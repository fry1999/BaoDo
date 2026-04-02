using BaoDo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BaoDo.API.Endpoints;

public static class RankingEndpoints
{
    public static void MapRankingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/ranking").WithTags("Ranking");

        group.MapGet("/exam", async (
            [FromQuery] string period,
            [FromQuery] int limit,
            IRankingService rankingService,
            CancellationToken ct) =>
        {
            var validPeriod = period is "weekly" or "monthly" or "all" ? period : "all";
            var safeLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 100);
            var board = await rankingService.GetExamLeaderboardAsync(validPeriod, safeLimit, ct);
            return Results.Ok(board);
        });

        group.MapGet("/xp", async (
            [FromQuery] int limit,
            IRankingService rankingService,
            CancellationToken ct) =>
        {
            var safeLimit = Math.Clamp(limit <= 0 ? 50 : limit, 1, 100);
            var board = await rankingService.GetXpLeaderboardAsync(safeLimit, ct);
            return Results.Ok(board);
        });

        group.MapGet("/me", async (
            [FromQuery] string period,
            IRankingService rankingService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException();
            var userId = Guid.Parse(claim.Value);
            var validPeriod = period is "weekly" or "monthly" or "all" ? period : "all";
            var result = await rankingService.GetMyRankAsync(userId, validPeriod, ct);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}
