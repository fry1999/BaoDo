using BaoDo.Core.Interfaces;
using BaoDo.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BaoDo.API.Endpoints;

public static class ExamEndpoints
{
    public static void MapExamEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/exam").WithTags("Exam").RequireAuthorization();

        group.MapGet("/", async (IExamService examService, CancellationToken ct) =>
        {
            var tests = await examService.GetTestsAsync(ct);
            return Results.Ok(tests);
        });

        group.MapGet("/history", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IExamService examService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            var safePage = page <= 0 ? 1 : page;
            var safeSize = Math.Clamp(pageSize <= 0 ? 10 : pageSize, 1, 50);
            var history = await examService.GetHistoryAsync(userId, safePage, safeSize, ct);
            return Results.Ok(history);
        });

        group.MapGet("/{testId:guid}", async (
            Guid testId,
            IExamService examService,
            CancellationToken ct) =>
        {
            var (test, questions) = await examService.GetTestWithQuestionsAsync(testId, ct);
            return Results.Ok(new { test, questions });
        });

        group.MapPost("/{testId:guid}/submit", async (
            Guid testId,
            [FromBody] SubmitExamRequest req,
            IExamService examService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            var result = await examService.SubmitExamAsync(userId, testId, req.Answers, ct);
            return Results.Ok(result);
        });

        group.MapGet("/results/{resultId:guid}", async (
            Guid resultId,
            IExamService examService,
            CancellationToken ct) =>
        {
            var result = await examService.GetResultAsync(resultId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim.Value);
    }

    public record SubmitExamRequest(IEnumerable<UserTestAnswer> Answers);
}
