using BaoDo.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace BaoDo.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<UserVocabularyCard> UserVocabularyCards => Set<UserVocabularyCard>();
    public DbSet<GrammarLesson> GrammarLessons => Set<GrammarLesson>();
    public DbSet<GrammarQuestion> GrammarQuestions => Set<GrammarQuestion>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Passage> Passages => Set<Passage>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<TestQuestion> TestQuestions => Set<TestQuestion>();
    public DbSet<UserTestResult> UserTestResults => Set<UserTestResult>();
    public DbSet<UserTestAnswer> UserTestAnswers => Set<UserTestAnswer>();
    public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();
    public DbSet<DictationContent> DictationContents => Set<DictationContent>();
    public DbSet<DictationSegment> DictationSegments => Set<DictationSegment>();
    public DbSet<UserDictationProgress> UserDictationProgress => Set<UserDictationProgress>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // UserProfile
        mb.Entity<UserProfile>(e =>
        {
            e.ToTable("profiles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(255);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(255);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Vocabulary
        mb.Entity<Vocabulary>(e =>
        {
            e.ToTable("vocabulary");
            e.HasKey(x => x.Id);
            e.Property(x => x.Examples).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!);
            e.Property(x => x.Collocations).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!);
        });

        // UserVocabularyCard
        mb.Entity<UserVocabularyCard>(e =>
        {
            e.ToTable("user_vocabulary_cards");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.VocabularyCards).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Vocabulary).WithMany(v => v.UserCards).HasForeignKey(x => x.VocabId);
            e.HasIndex(x => new { x.UserId, x.VocabId }).IsUnique();
        });

        // GrammarLesson
        mb.Entity<GrammarLesson>(e =>
        {
            e.ToTable("grammar_lessons");
            e.HasKey(x => x.Id);
        });

        mb.Entity<GrammarQuestion>(e =>
        {
            e.ToTable("grammar_questions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Options).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!);
            e.HasOne(x => x.Lesson).WithMany(l => l.Questions).HasForeignKey(x => x.LessonId);
        });

        // Question
        mb.Entity<Question>(e =>
        {
            e.ToTable("questions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Options).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!);
            e.Property(x => x.Tags).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!);
        });

        // Test  ("type" is a reserved word in PG — must quote it)
        mb.Entity<Test>(e =>
        {
            e.ToTable("tests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasColumnName("type");
        });

        mb.Entity<TestQuestion>(e =>
        {
            e.ToTable("test_questions");
            e.HasKey(x => new { x.TestId, x.QuestionId });
        });

        // UserTestResult
        mb.Entity<UserTestResult>(e =>
        {
            e.ToTable("user_test_results");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.TestResults).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Test).WithMany(t => t.Results).HasForeignKey(x => x.TestId);
        });

        mb.Entity<UserTestAnswer>(e =>
        {
            e.ToTable("user_test_answers");
            e.HasKey(x => x.Id);
        });

        // AIAnalysis
        mb.Entity<AIAnalysis>(e =>
        {
            e.ToTable("ai_analyses");
            e.HasKey(x => x.Id);
            e.Property(x => x.WeakParts).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<WeakPoint>>(v, (JsonSerializerOptions?)null)!);
            e.Property(x => x.StudyPlan).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<WeeklyPlan>>(v, (JsonSerializerOptions?)null)!);
            e.HasOne(x => x.Result).WithOne(r => r.AIAnalysis).HasForeignKey<AIAnalysis>(x => x.ResultId);
        });

        // Dictation
        mb.Entity<DictationContent>(e =>
        {
            e.ToTable("dictation_content");
            e.HasKey(x => x.Id);
        });

        mb.Entity<DictationSegment>(e =>
        {
            e.ToTable("dictation_segments");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Content).WithMany(c => c.Segments).HasForeignKey(x => x.ContentId);
        });

        mb.Entity<UserDictationProgress>(e =>
        {
            e.ToTable("user_dictation_progress");
            e.HasKey(x => x.Id);
        });

        // Subscription
        mb.Entity<Subscription>(e =>
        {
            e.ToTable("subscriptions");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.Subscriptions).HasForeignKey(x => x.UserId);
        });

        mb.Entity<Transaction>(e =>
        {
            e.ToTable("transactions");
            e.HasKey(x => x.Id);
        });

        // Global: store all C# enums as their string name in the DB (TEXT columns)
        foreach (var entityType in mb.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var clrType = property.ClrType;
                var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
                if (!underlyingType.IsEnum) continue;

                var converterType = typeof(EnumToStringConverter<>).MakeGenericType(underlyingType);
                var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                property.SetValueConverter(converter);
            }
        }
    }
}
