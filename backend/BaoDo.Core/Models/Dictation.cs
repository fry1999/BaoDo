namespace BaoDo.Core.Models;

public class DictationContent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DictationSource Source { get; set; }
    public string? YoutubeId { get; set; }
    public string? AudioUrl { get; set; }
    public DictationLevel Level { get; set; }
    public DictationTopic Topic { get; set; }
    public int Duration { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DictationSegment> Segments { get; set; } = [];
    public ICollection<UserDictationProgress> Progress { get; set; } = [];
}

public class DictationSegment
{
    public Guid Id { get; set; }
    public Guid ContentId { get; set; }
    public int Index { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
    public string Text { get; set; } = string.Empty;

    public DictationContent Content { get; set; } = null!;
}

public class UserDictationProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ContentId { get; set; }
    public int SegmentIndex { get; set; }
    public string UserInput { get; set; } = string.Empty;
    public int Accuracy { get; set; }
    public int Attempts { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum DictationSource { Library, YouTube }
public enum DictationLevel { Beginner, Intermediate, Advanced }
public enum DictationTopic { Business, Conversation, Announcement, News, Lecture }
