namespace BaoDo.Core.Models;

public record RankingEntry(
    int Rank,
    Guid UserId,
    string FullName,
    string? AvatarUrl,
    int Level,
    int BestScore,
    int TestsTaken,
    int XpTotal
);

public record UserRankResult(int Rank, RankingEntry? Entry);
