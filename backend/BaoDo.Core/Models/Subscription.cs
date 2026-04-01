namespace BaoDo.Core.Models;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserProfile User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public TransactionStatus Status { get; set; }
    public PaymentProvider Provider { get; set; }
    public string? ProviderRef { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserProfile User { get; set; } = null!;
    public Subscription? Subscription { get; set; }
}

public enum SubscriptionStatus { Active, Cancelled, Expired, Trial }
public enum TransactionStatus { Success, Pending, Failed, Refunded }
public enum PaymentProvider { PayOS, Stripe }
