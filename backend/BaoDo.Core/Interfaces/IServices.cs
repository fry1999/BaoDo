using BaoDo.Core.Models;

namespace BaoDo.Core.Interfaces;

public interface IVocabularyService
{
    Task<IEnumerable<UserVocabularyCard>> GetDueCardsAsync(Guid userId, CancellationToken ct = default);
    Task RateCardAsync(Guid userId, Guid cardId, int rating, CancellationToken ct = default);
    Task<UserVocabularyCard> SaveWordAsync(Guid userId, string word, CancellationToken ct = default);
}

public interface IGrammarService
{
    Task<IEnumerable<GrammarLesson>> GetLessonsAsync(CancellationToken ct = default);
    Task<GrammarLesson?> GetLessonAsync(Guid id, CancellationToken ct = default);
}

public interface IDictationService
{
    Task<IEnumerable<DictationContent>> GetLibraryAsync(CancellationToken ct = default);
    Task<DictationContent?> GetContentAsync(Guid id, CancellationToken ct = default);
    Task SaveProgressAsync(Guid userId, Guid contentId, int segmentIndex, string input, int accuracy, CancellationToken ct = default);
}

public interface IExamService
{
    Task<IEnumerable<Test>> GetTestsAsync(CancellationToken ct = default);
    Task<(Test test, IEnumerable<Question> questions)> GetTestWithQuestionsAsync(Guid testId, CancellationToken ct = default);
    Task<UserTestResult> SubmitExamAsync(Guid userId, Guid testId, IEnumerable<UserTestAnswer> answers, CancellationToken ct = default);
    Task<ExamResultDto?> GetResultAsync(Guid resultId, CancellationToken ct = default);
    Task<IEnumerable<ExamHistoryItem>> GetHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}

public interface IDictionaryService
{
    Task<DictionaryEntry?> LookupAsync(string word, CancellationToken ct = default);
}

public interface IAICoachService
{
    Task<AIAnalysis> GenerateAnalysisAsync(UserTestResult result, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<(string token, UserProfile user)> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<(string token, UserProfile user)> RegisterAsync(string email, string password, string fullName, CancellationToken ct = default);
    Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken ct = default);
}

public interface IRankingService
{
    Task<IEnumerable<RankingEntry>> GetExamLeaderboardAsync(string period, int limit, CancellationToken ct);
    Task<IEnumerable<RankingEntry>> GetXpLeaderboardAsync(int limit, CancellationToken ct);
    Task<UserRankResult> GetMyRankAsync(Guid userId, string period, CancellationToken ct);
}

/// <summary>Lightweight DTO returned by the dictionary service (not an EF entity)</summary>
public record DictionaryEntry(
    string Word,
    string Phonetic,
    string? AudioUrl,
    List<DictionaryMeaning> Meanings,
    List<string> Collocations,
    List<string> ToeicExamples
);

public record DictionaryMeaning(
    string PartOfSpeech,
    string DefinitionVi,
    string DefinitionEn,
    List<string> Examples
);
