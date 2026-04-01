namespace BaoDo.Core.Models;

public class Question
{
    public Guid Id { get; set; }
    public int Part { get; set; }   // SMALLINT in DB (1–7), not a PG enum
    public Difficulty Difficulty { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? QuestionText { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcript { get; set; }
    public List<string> Options { get; set; } = [];
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public Guid? PassageId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Passage? Passage { get; set; }
    public ICollection<TestQuestion> TestQuestions { get; set; } = [];
}

public class Passage
{
    public Guid Id { get; set; }
    public short Part { get; set; }  // SMALLINT in DB
    public PassageType PassageType { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; } = [];
}

public enum Difficulty { Easy, Medium, Hard }
public enum PassageType { Single, Double, Triple }
