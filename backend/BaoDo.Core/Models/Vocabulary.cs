namespace BaoDo.Core.Models;

public class Vocabulary
{
    public Guid Id { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Phonetic { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string MeaningVi { get; set; } = string.Empty;
    public string MeaningEn { get; set; } = string.Empty;
    public PartOfSpeech PartOfSpeech { get; set; }
    public List<string> Examples { get; set; } = [];
    public List<string> Collocations { get; set; } = [];
    public VocabTopic Topic { get; set; }
    public VocabLevel Level { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserVocabularyCard> UserCards { get; set; } = [];
}

public class UserVocabularyCard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VocabId { get; set; }
    public SRSStatus Status { get; set; } = SRSStatus.New;
    public int Interval { get; set; } = 1;
    public double EaseFactor { get; set; } = 2.5;
    public int Repetitions { get; set; }
    public DateTime NextReviewAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }

    public UserProfile User { get; set; } = null!;
    public Vocabulary Vocabulary { get; set; } = null!;
}

public enum PartOfSpeech { Noun, Verb, Adjective, Adverb, Preposition, Conjunction, Phrase }
public enum VocabTopic { Business, Finance, Office, Travel, Technology, HumanResources, Marketing, Legal, General }
public enum VocabLevel { Basic, Intermediate, Advanced }
public enum SRSStatus { New, Learning, Review, Mastered }
