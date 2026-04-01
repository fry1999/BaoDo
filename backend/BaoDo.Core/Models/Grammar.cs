namespace BaoDo.Core.Models;

public class GrammarLesson
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public GrammarCategory Category { get; set; }
    public string Content { get; set; } = string.Empty;
    public short Difficulty { get; set; } = 1;  // SMALLINT in DB (1–3), not a PG enum
    public int EstimatedMinutes { get; set; } = 10;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GrammarQuestion> Questions { get; set; } = [];
}

public class GrammarQuestion
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public GrammarLesson Lesson { get; set; } = null!;
}

public enum GrammarCategory
{
    Tenses,
    WordForm,
    Prepositions,
    Conjunctions,
    Articles,
    SubjectVerbAgreement,
    RelativeClauses,
    Conditionals,
    PassiveVoice,
    Comparison
}
