using BaoDo.Core.Interfaces;
using BaoDo.Core.Models;
using BaoDo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaoDo.Infrastructure.Services;

public class VocabularyService(AppDbContext db) : IVocabularyService
{
    public async Task<IEnumerable<UserVocabularyCard>> GetDueCardsAsync(Guid userId, CancellationToken ct)
        => await db.UserVocabularyCards
            .Include(c => c.Vocabulary)
            .Where(c => c.UserId == userId && c.NextReviewAt <= DateTime.UtcNow)
            .OrderBy(c => c.NextReviewAt)
            .Take(20)
            .ToListAsync(ct);

    public async Task RateCardAsync(Guid userId, Guid cardId, int rating, CancellationToken ct)
    {
        var card = await db.UserVocabularyCards
            .FirstOrDefaultAsync(c => c.Id == cardId && c.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Card not found");

        var (interval, easeFactor, repetitions) = ApplySM2(rating, card.Repetitions, card.Interval, card.EaseFactor);

        card.Interval = interval;
        card.EaseFactor = easeFactor;
        card.Repetitions = repetitions;
        card.NextReviewAt = DateTime.UtcNow.AddDays(interval);
        card.LastReviewedAt = DateTime.UtcNow;
        card.Status = repetitions >= 5 ? SRSStatus.Mastered : rating >= 3 ? SRSStatus.Review : SRSStatus.Learning;

        await db.SaveChangesAsync(ct);
    }

    public async Task<UserVocabularyCard> SaveWordAsync(Guid userId, string word, CancellationToken ct)
    {
        var vocab = await db.Vocabularies.FirstOrDefaultAsync(v => v.Word == word.ToLower(), ct)
            ?? throw new KeyNotFoundException($"Word '{word}' not found in vocabulary bank");

        var existing = await db.UserVocabularyCards
            .FirstOrDefaultAsync(c => c.UserId == userId && c.VocabId == vocab.Id, ct);

        if (existing is not null) return existing;

        var card = new UserVocabularyCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabId = vocab.Id,
            Status = SRSStatus.New,
            NextReviewAt = DateTime.UtcNow,
        };

        db.UserVocabularyCards.Add(card);
        await db.SaveChangesAsync(ct);
        return card;
    }

    /// SM-2 algorithm
    private static (int interval, double easeFactor, int repetitions) ApplySM2(
        int rating, int repetitions, int interval, double easeFactor)
    {
        const double minEase = 1.3;
        var newEase = easeFactor + (0.1 - (5 - rating) * (0.08 + (5 - rating) * 0.02));
        if (newEase < minEase) newEase = minEase;

        if (rating < 3) return (1, newEase, 0);

        int nextInterval = repetitions switch
        {
            0 => 1,
            1 => 6,
            _ => (int)Math.Round(interval * newEase),
        };

        return (nextInterval, newEase, repetitions + 1);
    }
}
