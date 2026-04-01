using BaoDo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BaoDo.API.Endpoints;

public static class VocabularyEndpoints
{
    public static void MapVocabularyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/vocabulary").WithTags("Vocabulary").RequireAuthorization();

        group.MapGet("/due", async (
            IVocabularyService vocabService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            var cards = await vocabService.GetDueCardsAsync(userId, ct);
            return Results.Ok(cards);
        });

        group.MapPost("/cards/{cardId:guid}/rate", async (
            Guid cardId,
            [FromBody] RateCardRequest req,
            IVocabularyService vocabService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            await vocabService.RateCardAsync(userId, cardId, req.Rating, ct);
            return Results.Ok();
        });

        group.MapPost("/save", async (
            [FromBody] SaveWordRequest req,
            IVocabularyService vocabService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            var card = await vocabService.SaveWordAsync(userId, req.Word, ct);
            return Results.Ok(card);
        });
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim.Value);
    }

    public record RateCardRequest(int Rating);
    public record SaveWordRequest(string Word);
}
