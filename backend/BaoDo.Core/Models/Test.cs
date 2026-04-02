namespace BaoDo.Core.Models;

public class Test
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TestType Type { get; set; }
    public int TotalQuestions { get; set; }
    public int DurationMinutes { get; set; } = 120;
    public Difficulty Difficulty { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TestQuestion> TestQuestions { get; set; } = [];
    public ICollection<UserTestResult> Results { get; set; } = [];
}

public class TestQuestion
{
    public Guid TestId { get; set; }
    public Guid QuestionId { get; set; }
    public int SortOrder { get; set; }

    public Test Test { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

public class UserTestResult
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TestId { get; set; }
    public int ListeningRaw { get; set; }
    public int ReadingRaw { get; set; }
    public int ListeningScaled { get; set; }
    public int ReadingScaled { get; set; }
    public int TotalScore { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public UserProfile User { get; set; } = null!;
    public Test Test { get; set; } = null!;
    public ICollection<UserTestAnswer> Answers { get; set; } = [];
    public AIAnalysis? AIAnalysis { get; set; }
}

public class UserTestAnswer
{
    public Guid Id { get; set; }
    public Guid ResultId { get; set; }
    public Guid QuestionId { get; set; }
    public int SelectedIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int TimeSpentSeconds { get; set; }

    public UserTestResult Result { get; set; } = null!;
}

public class AIAnalysis
{
    public Guid Id { get; set; }
    public Guid ResultId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<WeakPoint> WeakParts { get; set; } = [];
    public List<WeeklyPlan> StudyPlan { get; set; } = [];
    public int DailyMinutes { get; set; }
    public int PredictedScore { get; set; }
    public int SuccessProbability { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public UserTestResult Result { get; set; } = null!;
}

public record WeakPoint(int Part, int Accuracy, string PriorityLevel, string Recommendation);
public record WeeklyPlan(int Week, string Focus, List<string> Tasks);

public record PartScore(int Part, int Correct, int Total, int Percentage);

public record ExamResultDto(
    Guid Id, Guid UserId, Guid TestId,
    int ListeningRaw, int ReadingRaw,
    int ListeningScaled, int ReadingScaled,
    int TotalScore, int DurationSeconds,
    DateTime CompletedAt,
    ICollection<UserTestAnswer> Answers,
    AIAnalysis? AiAnalysis,
    List<PartScore> PartBreakdown
);

public record ExamHistoryItem(
    Guid Id, Guid TestId, string TestTitle,
    int ListeningScaled, int ReadingScaled,
    int TotalScore, DateTime CompletedAt
);

public enum TestType { Full, Mini, Part }
