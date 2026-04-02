using BaoDo.Core.Interfaces;
using BaoDo.Core.Models;
using BaoDo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BaoDo.Infrastructure.Services;

public class RankingService(AppDbContext db) : IRankingService
{
    public async Task<IEnumerable<RankingEntry>> GetExamLeaderboardAsync(
        string period, int limit, CancellationToken ct)
    {
        var cutoff = GetCutoff(period);

        var query = db.UserTestResults.AsQueryable();
        if (cutoff.HasValue)
            query = query.Where(r => r.CompletedAt >= cutoff.Value);

        var grouped = await query
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                BestScore = g.Max(r => r.TotalScore),
                TestsTaken = g.Count(),
            })
            .OrderByDescending(x => x.BestScore)
            .Take(limit)
            .ToListAsync(ct);

        var userIds = grouped.Select(g => g.UserId).ToList();
        var profiles = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        return grouped.Select((g, idx) =>
        {
            profiles.TryGetValue(g.UserId, out var profile);
            return new RankingEntry(
                Rank: idx + 1,
                UserId: g.UserId,
                FullName: profile?.FullName ?? "Unknown",
                AvatarUrl: profile?.AvatarUrl,
                Level: profile?.Level ?? 1,
                BestScore: g.BestScore,
                TestsTaken: g.TestsTaken,
                XpTotal: profile?.XpTotal ?? 0
            );
        });
    }

    public async Task<IEnumerable<RankingEntry>> GetXpLeaderboardAsync(
        int limit, CancellationToken ct)
    {
        var profiles = await db.Users
            .Where(u => u.Role != UserRole.Banned)
            .OrderByDescending(u => u.XpTotal)
            .Take(limit)
            .ToListAsync(ct);

        return profiles.Select((p, idx) => new RankingEntry(
            Rank: idx + 1,
            UserId: p.Id,
            FullName: p.FullName,
            AvatarUrl: p.AvatarUrl,
            Level: p.Level,
            BestScore: 0,
            TestsTaken: 0,
            XpTotal: p.XpTotal
        ));
    }

    public async Task<UserRankResult> GetMyRankAsync(
        Guid userId, string period, CancellationToken ct)
    {
        var cutoff = GetCutoff(period);

        var query = db.UserTestResults.AsQueryable();
        if (cutoff.HasValue)
            query = query.Where(r => r.CompletedAt >= cutoff.Value);

        var allGrouped = await query
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, BestScore = g.Max(r => r.TotalScore), TestsTaken = g.Count() })
            .OrderByDescending(x => x.BestScore)
            .ToListAsync(ct);

        var myIdx = allGrouped.FindIndex(g => g.UserId == userId);
        if (myIdx < 0) return new UserRankResult(0, null);

        var my = allGrouped[myIdx];
        var profile = await db.Users.FindAsync([userId], ct);

        var entry = new RankingEntry(
            Rank: myIdx + 1,
            UserId: userId,
            FullName: profile?.FullName ?? "Unknown",
            AvatarUrl: profile?.AvatarUrl,
            Level: profile?.Level ?? 1,
            BestScore: my.BestScore,
            TestsTaken: my.TestsTaken,
            XpTotal: profile?.XpTotal ?? 0
        );

        return new UserRankResult(myIdx + 1, entry);
    }

    private static DateTime? GetCutoff(string period) => period switch
    {
        "weekly" => DateTime.UtcNow.AddDays(-7),
        "monthly" => DateTime.UtcNow.AddDays(-30),
        _ => null,
    };
}
