using BaoDo.Core.Interfaces;
using BaoDo.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaoDo.API.Endpoints;

public static class OtherEndpoints
{
    public static void MapGrammarEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/grammar").WithTags("Grammar").RequireAuthorization();

        group.MapGet("/", async (IGrammarService grammarService, CancellationToken ct) =>
            Results.Ok(await grammarService.GetLessonsAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IGrammarService grammarService, CancellationToken ct) =>
        {
            var lesson = await grammarService.GetLessonAsync(id, ct);
            return lesson is null ? Results.NotFound() : Results.Ok(lesson);
        });
    }

    public static void MapDictationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dictation").WithTags("Dictation").RequireAuthorization();

        group.MapGet("/", async (IDictationService dictationService, CancellationToken ct) =>
            Results.Ok(await dictationService.GetLibraryAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IDictationService dictationService, CancellationToken ct) =>
        {
            var content = await dictationService.GetContentAsync(id, ct);
            return content is null ? Results.NotFound() : Results.Ok(content);
        });

        group.MapPost("/{id:guid}/progress", async (
            Guid id,
            [FromBody] DictationProgressRequest req,
            IDictationService dictationService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            await dictationService.SaveProgressAsync(userId, id, req.SegmentIndex, req.UserInput, req.Accuracy, ct);
            return Results.Ok();
        });
    }

    public static void MapPracticeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/practice").WithTags("Practice").RequireAuthorization();

        group.MapGet("/", async (
            [FromQuery] int part,
            [FromQuery] int limit,
            BaoDo.Infrastructure.Data.AppDbContext db,
            CancellationToken ct) =>
        {
            var questions = await db.Questions
                .Where(q => (int)q.Part == part && q.IsPublished)
                .OrderBy(q => Guid.NewGuid())
                .Take(Math.Min(limit, 50))
                .ToListAsync(ct);
            return Results.Ok(questions);
        });

        group.MapPost("/session", async (
            [FromBody] List<PracticeAnswerRequest> answers,
            BaoDo.Infrastructure.Data.AppDbContext db,
            CancellationToken ct) =>
        {
            var questionIds = answers.Select(a => a.QuestionId).ToList();
            var questions = await db.Questions
                .Where(q => questionIds.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id, ct);

            var results = answers.Select(a =>
            {
                questions.TryGetValue(a.QuestionId, out var q);
                var isCorrect = q is not null && a.SelectedIndex == q.CorrectIndex;
                return new
                {
                    a.QuestionId,
                    a.SelectedIndex,
                    IsCorrect = isCorrect,
                    CorrectIndex = q?.CorrectIndex ?? -1,
                    Explanation = q?.Explanation ?? string.Empty,
                    QuestionText = q?.QuestionText,
                    Options = q?.Options ?? [],
                };
            }).ToList();

            var correct = results.Count(r => r.IsCorrect);
            var total = results.Count;
            return Results.Ok(new
            {
                Correct = correct,
                Total = total,
                Percentage = total > 0 ? (int)Math.Round((double)correct / total * 100) : 0,
                Items = results,
            });
        });
    }

    public record PracticeAnswerRequest(Guid QuestionId, int SelectedIndex);

    public static void MapDictionaryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dictionary").WithTags("Dictionary");

        group.MapGet("/lookup", async (
            [FromQuery] string word,
            IDictionaryService dictionaryService,
            CancellationToken ct) =>
        {
            var entry = await dictionaryService.LookupAsync(word, ct);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        });
    }

    public static void MapAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization("AdminOnly");

        group.MapGet("/stats", async (BaoDo.Infrastructure.Data.AppDbContext db, CancellationToken ct) =>
        {
            var utcNow = DateTime.UtcNow;
            var startOfTodayUtc = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc);
            var startOfMonthUtc = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var stats = new
            {
                TotalUsers = await db.Users.CountAsync(ct),
                NewUsersToday = await db.Users.CountAsync(u => u.CreatedAt >= startOfTodayUtc, ct),
                ActiveUsersToday = await db.Users.CountAsync(u => u.LastActiveAt >= startOfTodayUtc, ct),
                TestsCompletedToday = await db.UserTestResults.CountAsync(r => r.CompletedAt >= startOfTodayUtc, ct),
                RevenueToday = await db.Transactions
                    .Where(t => t.CreatedAt >= startOfTodayUtc && t.Status == Core.Models.TransactionStatus.Success)
                    .SumAsync(t => t.Amount, ct),
                RevenueThisMonth = await db.Transactions
                    .Where(t => t.CreatedAt >= startOfMonthUtc && t.Status == Core.Models.TransactionStatus.Success)
                    .SumAsync(t => t.Amount, ct),
            };
            return Results.Ok(stats);
        });

        group.MapGet("/users", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            BaoDo.Infrastructure.Data.AppDbContext db,
            CancellationToken ct) =>
        {
            var query = db.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var total = await query.CountAsync(ct);
            var data = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(new { data, total, page, pageSize, totalPages = (int)Math.Ceiling((double)total / pageSize) });
        });

        group.MapPost("/users/{userId:guid}/ban", async (
            Guid userId,
            BaoDo.Infrastructure.Data.AppDbContext db,
            CancellationToken ct) =>
        {
            var user = await db.Users.FindAsync([userId], ct);
            if (user is null) return Results.NotFound();
            if (user.Role == BaoDo.Core.Models.UserRole.Admin) return Results.BadRequest("Không thể khóa tài khoản admin");
            user.Role = BaoDo.Core.Models.UserRole.Banned;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { message = "Tài khoản đã bị khóa" });
        });

        group.MapPost("/users/{userId:guid}/unban", async (
            Guid userId,
            BaoDo.Infrastructure.Data.AppDbContext db,
            CancellationToken ct) =>
        {
            var user = await db.Users.FindAsync([userId], ct);
            if (user is null) return Results.NotFound();
            if (user.Role != BaoDo.Core.Models.UserRole.Banned) return Results.BadRequest("Tài khoản không bị khóa");
            user.Role = BaoDo.Core.Models.UserRole.User;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { message = "Tài khoản đã được mở khóa" });
        });
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim.Value);
    }

    public record DictationProgressRequest(int SegmentIndex, string UserInput, int Accuracy);
}
