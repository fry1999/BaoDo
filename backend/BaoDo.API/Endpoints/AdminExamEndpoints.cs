using BaoDo.Core.Models;
using BaoDo.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaoDo.API.Endpoints;

public static class AdminExamEndpoints
{
    public static void MapAdminExamEndpoints(this WebApplication app)
    {
        MapQuestionEndpoints(app);
        MapPassageEndpoints(app);
        MapTestManagementEndpoints(app);
    }

    // ─── Questions ────────────────────────────────────────────────────────────

    private static void MapQuestionEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/admin/questions").WithTags("Admin-Questions")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (
            [FromQuery] int? part,
            [FromQuery] string? difficulty,
            [FromQuery] bool? published,
            [FromQuery] string? search,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var safePage = page <= 0 ? 1 : page;
            var safeSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 100);

            var query = db.Questions.Include(q => q.Passage).AsQueryable();

            if (part.HasValue) query = query.Where(q => q.Part == part.Value);
            if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<Difficulty>(difficulty, true, out var diff))
                query = query.Where(q => q.Difficulty == diff);
            if (published.HasValue) query = query.Where(q => q.IsPublished == published.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(q => q.QuestionText != null && q.QuestionText.Contains(search));

            var total = await query.CountAsync(ct);
            var data = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .ToListAsync(ct);

            return Results.Ok(new { data, total, page = safePage, pageSize = safeSize,
                totalPages = (int)Math.Ceiling((double)total / safeSize) });
        });

        group.MapPost("/", async (
            [FromBody] QuestionUpsertRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Part = req.Part,
                Difficulty = Enum.TryParse<Difficulty>(req.Difficulty, true, out var diff) ? diff : Difficulty.Medium,
                Tags = req.Tags ?? [],
                QuestionText = req.QuestionText,
                ImageUrl = req.ImageUrl,
                AudioUrl = req.AudioUrl,
                Transcript = req.Transcript,
                Options = req.Options,
                CorrectIndex = req.CorrectIndex,
                Explanation = req.Explanation,
                PassageId = req.PassageId,
                IsPublished = false,
            };
            db.Questions.Add(question);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/questions/{question.Id}", question);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] QuestionUpsertRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var question = await db.Questions.FindAsync([id], ct);
            if (question is null) return Results.NotFound();

            question.Part = req.Part;
            question.Difficulty = Enum.TryParse<Difficulty>(req.Difficulty, true, out var diff) ? diff : Difficulty.Medium;
            question.Tags = req.Tags ?? [];
            question.QuestionText = req.QuestionText;
            question.ImageUrl = req.ImageUrl;
            question.AudioUrl = req.AudioUrl;
            question.Transcript = req.Transcript;
            question.Options = req.Options;
            question.CorrectIndex = req.CorrectIndex;
            question.Explanation = req.Explanation;
            question.PassageId = req.PassageId;

            await db.SaveChangesAsync(ct);
            return Results.Ok(question);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var question = await db.Questions.FindAsync([id], ct);
            if (question is null) return Results.NotFound();
            db.Questions.Remove(question);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPatch("/{id:guid}/publish", async (
            Guid id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var question = await db.Questions.FindAsync([id], ct);
            if (question is null) return Results.NotFound();
            question.IsPublished = !question.IsPublished;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id, isPublished = question.IsPublished });
        });
    }

    // ─── Passages ─────────────────────────────────────────────────────────────

    private static void MapPassageEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/admin/passages").WithTags("Admin-Passages")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (
            [FromQuery] int? part,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var safePage = page <= 0 ? 1 : page;
            var safeSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 100);

            var query = db.Passages.AsQueryable();
            if (part.HasValue) query = query.Where(p => p.Part == part.Value);

            var total = await query.CountAsync(ct);
            var data = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .ToListAsync(ct);

            return Results.Ok(new { data, total, page = safePage, pageSize = safeSize,
                totalPages = (int)Math.Ceiling((double)total / safeSize) });
        });

        group.MapPost("/", async (
            [FromBody] PassageUpsertRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var passage = new Passage
            {
                Id = Guid.NewGuid(),
                Part = (short)req.Part,
                PassageType = Enum.TryParse<PassageType>(req.PassageType, true, out var pt) ? pt : PassageType.Single,
                Title = req.Title,
                Content = req.Content,
            };
            db.Passages.Add(passage);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/passages/{passage.Id}", passage);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] PassageUpsertRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var passage = await db.Passages.FindAsync([id], ct);
            if (passage is null) return Results.NotFound();

            passage.Part = (short)req.Part;
            passage.PassageType = Enum.TryParse<PassageType>(req.PassageType, true, out var pt) ? pt : PassageType.Single;
            passage.Title = req.Title;
            passage.Content = req.Content;

            await db.SaveChangesAsync(ct);
            return Results.Ok(passage);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var passage = await db.Passages.FindAsync([id], ct);
            if (passage is null) return Results.NotFound();
            db.Passages.Remove(passage);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    private static void MapTestManagementEndpoints(WebApplication app)
    {
        var group = app.MapGroup("/api/admin/tests").WithTags("Admin-Tests")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (
            [FromQuery] bool? published,
            [FromQuery] string? type,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var safePage = page <= 0 ? 1 : page;
            var safeSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 100);

            var query = db.Tests.AsQueryable();
            if (published.HasValue) query = query.Where(t => t.IsPublished == published.Value);
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<TestType>(type, true, out var tt))
                query = query.Where(t => t.Type == tt);

            var total = await query.CountAsync(ct);
            var data = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .ToListAsync(ct);

            return Results.Ok(new { data, total, page = safePage, pageSize = safeSize,
                totalPages = (int)Math.Ceiling((double)total / safeSize) });
        });

        group.MapPost("/", async (
            [FromBody] TestUpsertRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var test = new Test
            {
                Id = Guid.NewGuid(),
                Title = req.Title,
                Type = Enum.TryParse<TestType>(req.Type, true, out var tt) ? tt : TestType.Full,
                TotalQuestions = req.TotalQuestions,
                DurationMinutes = req.DurationMinutes,
                Difficulty = Enum.TryParse<Difficulty>(req.Difficulty, true, out var diff) ? diff : Difficulty.Medium,
                IsPublished = false,
            };
            db.Tests.Add(test);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/admin/tests/{test.Id}", test);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] TestUpsertRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var test = await db.Tests.FindAsync([id], ct);
            if (test is null) return Results.NotFound();

            test.Title = req.Title;
            test.Type = Enum.TryParse<TestType>(req.Type, true, out var tt) ? tt : TestType.Full;
            test.TotalQuestions = req.TotalQuestions;
            test.DurationMinutes = req.DurationMinutes;
            test.Difficulty = Enum.TryParse<Difficulty>(req.Difficulty, true, out var diff) ? diff : Difficulty.Medium;

            await db.SaveChangesAsync(ct);
            return Results.Ok(test);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var test = await db.Tests.FindAsync([id], ct);
            if (test is null) return Results.NotFound();
            db.Tests.Remove(test);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPatch("/{id:guid}/publish", async (
            Guid id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var test = await db.Tests.FindAsync([id], ct);
            if (test is null) return Results.NotFound();
            test.IsPublished = !test.IsPublished;
            test.PublishedAt = test.IsPublished ? DateTime.UtcNow : null;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id, isPublished = test.IsPublished });
        });

        group.MapGet("/{id:guid}/questions", async (
            Guid id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var questions = await db.TestQuestions
                .Where(tq => tq.TestId == id)
                .OrderBy(tq => tq.SortOrder)
                .Include(tq => tq.Question)
                .Select(tq => new { tq.SortOrder, Question = tq.Question })
                .ToListAsync(ct);
            return Results.Ok(questions);
        });

        group.MapPost("/{id:guid}/questions", async (
            Guid id,
            [FromBody] AddQuestionRequest req,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var exists = await db.TestQuestions
                .AnyAsync(tq => tq.TestId == id && tq.QuestionId == req.QuestionId, ct);
            if (exists) return Results.Conflict(new { message = "Câu hỏi đã có trong đề thi" });

            var maxOrder = await db.TestQuestions
                .Where(tq => tq.TestId == id)
                .MaxAsync(tq => (int?)tq.SortOrder, ct) ?? 0;

            db.TestQuestions.Add(new TestQuestion
            {
                TestId = id,
                QuestionId = req.QuestionId,
                SortOrder = maxOrder + 1,
            });

            var test = await db.Tests.FindAsync([id], ct);
            if (test is not null) test.TotalQuestions++;

            await db.SaveChangesAsync(ct);
            return Results.Ok();
        });

        group.MapDelete("/{id:guid}/questions/{questionId:guid}", async (
            Guid id,
            Guid questionId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var tq = await db.TestQuestions
                .FirstOrDefaultAsync(x => x.TestId == id && x.QuestionId == questionId, ct);
            if (tq is null) return Results.NotFound();

            db.TestQuestions.Remove(tq);

            var test = await db.Tests.FindAsync([id], ct);
            if (test is not null && test.TotalQuestions > 0) test.TotalQuestions--;

            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        group.MapPut("/{id:guid}/questions/reorder", async (
            Guid id,
            [FromBody] List<ReorderItem> items,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var tqs = await db.TestQuestions.Where(tq => tq.TestId == id).ToListAsync(ct);
            foreach (var item in items)
            {
                var tq = tqs.FirstOrDefault(x => x.QuestionId == item.QuestionId);
                if (tq is not null) tq.SortOrder = item.SortOrder;
            }
            await db.SaveChangesAsync(ct);
            return Results.Ok();
        });
    }

    // ─── Request records ──────────────────────────────────────────────────────

    public record QuestionUpsertRequest(
        int Part, string Difficulty, string? QuestionText,
        string? ImageUrl, string? AudioUrl, string? Transcript,
        List<string> Options, int CorrectIndex, string Explanation,
        List<string>? Tags, Guid? PassageId);

    public record PassageUpsertRequest(
        int Part, string PassageType, string? Title, string Content);

    public record TestUpsertRequest(
        string Title, string Type, int TotalQuestions,
        int DurationMinutes, string Difficulty);

    public record AddQuestionRequest(Guid QuestionId);

    public record ReorderItem(Guid QuestionId, int SortOrder);
}
