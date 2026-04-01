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
