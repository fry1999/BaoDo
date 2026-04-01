namespace BaoDo.Core.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public SubscriptionPlan Subscription { get; set; } = SubscriptionPlan.Free;
    public int TargetScore { get; set; } = 700;
    public DateOnly? ExamDate { get; set; }
    public int StreakCount { get; set; }
    public int XpTotal { get; set; }
    public int Level { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActiveAt { get; set; }

    // Navigation
    public ICollection<UserVocabularyCard> VocabularyCards { get; set; } = [];
    public ICollection<UserTestResult> TestResults { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
}

public enum UserRole { User, ContentEditor, Admin }
public enum SubscriptionPlan { Free, Basic, Pro }
