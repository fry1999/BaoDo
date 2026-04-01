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
    }

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
        var group = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization();

        group.MapGet("/stats", async (BaoDo.Infrastructure.Data.AppDbContext db, CancellationToken ct) =>
        {
            var now = DateTime.UtcNow.Date;
            var stats = new
            {
                TotalUsers = await db.Users.CountAsync(ct),
                NewUsersToday = await db.Users.CountAsync(u => u.CreatedAt >= now, ct),
                ActiveUsersToday = await db.Users.CountAsync(u => u.LastActiveAt >= now, ct),
                TestsCompletedToday = await db.UserTestResults.CountAsync(r => r.CompletedAt >= now, ct),
                RevenueToday = await db.Transactions
                    .Where(t => t.CreatedAt >= now && t.Status == Core.Models.TransactionStatus.Success)
                    .SumAsync(t => t.Amount, ct),
                RevenueThisMonth = await db.Transactions
                    .Where(t => t.CreatedAt >= new DateTime(now.Year, now.Month, 1) && t.Status == Core.Models.TransactionStatus.Success)
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
            // Ban logic: set role to indicate banned state (extend model as needed)
            await db.SaveChangesAsync(ct);
            return Results.Ok();
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
