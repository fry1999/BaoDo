using BaoDo.Core.Interfaces;
using BaoDo.Core.Models;
using BaoDo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text.Json;

namespace BaoDo.Infrastructure.Services;

public class GrammarService(AppDbContext db) : IGrammarService
{
    public async Task<IEnumerable<GrammarLesson>> GetLessonsAsync(CancellationToken ct)
        => await db.GrammarLessons
            .Where(l => l.IsPublished)
            .Include(l => l.Questions)
            .OrderBy(l => l.Category)
            .ThenBy(l => l.Difficulty)
            .ToListAsync(ct);

    public async Task<GrammarLesson?> GetLessonAsync(Guid id, CancellationToken ct)
        => await db.GrammarLessons
            .Include(l => l.Questions)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
}

public class DictationService(AppDbContext db) : IDictationService
{
    public async Task<IEnumerable<DictationContent>> GetLibraryAsync(CancellationToken ct)
        => await db.DictationContents
            .Where(d => d.IsPublished)
            .Include(d => d.Segments)
            .OrderBy(d => d.Level)
            .ToListAsync(ct);

    public async Task<DictationContent?> GetContentAsync(Guid id, CancellationToken ct)
        => await db.DictationContents
            .Include(d => d.Segments.OrderBy(s => s.Index))
            .FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task SaveProgressAsync(
        Guid userId, Guid contentId, int segmentIndex,
        string input, int accuracy, CancellationToken ct)
    {
        var existing = await db.UserDictationProgress.FirstOrDefaultAsync(
            p => p.UserId == userId && p.ContentId == contentId && p.SegmentIndex == segmentIndex, ct);

        if (existing is null)
        {
            db.UserDictationProgress.Add(new UserDictationProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ContentId = contentId,
                SegmentIndex = segmentIndex,
                UserInput = input,
                Accuracy = accuracy,
                Attempts = 1,
                CompletedAt = accuracy >= 90 ? DateTime.UtcNow : null,
            });
        }
        else
        {
            existing.UserInput = input;
            existing.Accuracy = Math.Max(existing.Accuracy, accuracy);
            existing.Attempts++;
            if (accuracy >= 90) existing.CompletedAt ??= DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}

public class ExamService(AppDbContext db, IAICoachService aiCoach) : IExamService
{
    public async Task<IEnumerable<Test>> GetTestsAsync(CancellationToken ct)
        => await db.Tests.Where(t => t.IsPublished).OrderByDescending(t => t.PublishedAt).ToListAsync(ct);

    public async Task<(Test test, IEnumerable<Question> questions)> GetTestWithQuestionsAsync(Guid testId, CancellationToken ct)
    {
        var test = await db.Tests.FindAsync([testId], ct)
            ?? throw new KeyNotFoundException("Test not found");

        var questions = await db.TestQuestions
            .Where(tq => tq.TestId == testId)
            .OrderBy(tq => tq.SortOrder)
            .Include(tq => tq.Question)
            .Select(tq => tq.Question)
            .ToListAsync(ct);

        return (test, questions);
    }

    public async Task<UserTestResult> SubmitExamAsync(
        Guid userId, Guid testId,
        IEnumerable<UserTestAnswer> answers,
        CancellationToken ct)
    {
        var (test, questions) = await GetTestWithQuestionsAsync(testId, ct);
        var questionList = questions.ToList();
        var answerList = answers.ToList();

        var listeningRaw = questionList
            .Where(q => (int)q.Part <= 4)
            .Count(q => answerList.FirstOrDefault(a => a.QuestionId == q.Id)?.IsCorrect == true);

        var readingRaw = questionList
            .Where(q => (int)q.Part >= 5)
            .Count(q => answerList.FirstOrDefault(a => a.QuestionId == q.Id)?.IsCorrect == true);

        var (listeningScaled, readingScaled) = ConvertToScaled(listeningRaw, readingRaw);

        var result = new UserTestResult
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TestId = testId,
            ListeningRaw = listeningRaw,
            ReadingRaw = readingRaw,
            ListeningScaled = listeningScaled,
            ReadingScaled = readingScaled,
            TotalScore = listeningScaled + readingScaled,
            CompletedAt = DateTime.UtcNow,
        };

        foreach (var a in answerList)
        {
            a.Id = Guid.NewGuid();
            a.ResultId = result.Id;
        }
        result.Answers = answerList;

        db.UserTestResults.Add(result);
        await db.SaveChangesAsync(ct);

        // Generate AI analysis asynchronously (fire and forget for fast response)
        _ = Task.Run(async () =>
        {
            var analysis = await aiCoach.GenerateAnalysisAsync(result, CancellationToken.None);
            db.AIAnalyses.Add(analysis);
            await db.SaveChangesAsync(CancellationToken.None);
        });

        return result;
    }

    public async Task<UserTestResult?> GetResultAsync(Guid resultId, CancellationToken ct)
        => await db.UserTestResults
            .Include(r => r.Answers)
            .Include(r => r.AIAnalysis)
            .FirstOrDefaultAsync(r => r.Id == resultId, ct);

    /// Simplified TOEIC score conversion (use official ETS conversion table in production)
    private static (int listening, int reading) ConvertToScaled(int listeningRaw, int readingRaw)
    {
        int ListeningScale(int raw) => (int)Math.Round(5 + (raw / 100.0) * 490);
        int ReadingScale(int raw) => (int)Math.Round(5 + (raw / 100.0) * 490);
        return (ListeningScale(listeningRaw), ReadingScale(readingRaw));
    }
}

public class DictionaryService(IDistributedCache cache) : IDictionaryService
{
    public async Task<DictionaryEntry?> LookupAsync(string word, CancellationToken ct)
    {
        var cacheKey = $"dict:{word.ToLower()}";
        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<DictionaryEntry>(cached);

        // Fallback: return null (connect to OpenAI or dictionary API in production)
        return null;
    }
}

public class AICoachService(IConfiguration config) : IAICoachService
{
    public async Task<AIAnalysis> GenerateAnalysisAsync(UserTestResult result, CancellationToken ct)
    {
        var apiKey = config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return new AIAnalysis
            {
                Id = Guid.NewGuid(),
                ResultId = result.Id,
                Summary = "Phân tích AI chưa được cấu hình.",
                WeakParts = [],
                StudyPlan = [],
                DailyMinutes = 45,
                PredictedScore = result.TotalScore + 50,
                SuccessProbability = 65,
            };
        }

        var client = new ChatClient("gpt-4o-mini", apiKey);
        var prompt = BuildPrompt(result);

        var chatResult = await client.CompleteChatAsync(
        [
            ChatMessage.CreateSystemMessage("Bạn là AI coach TOEIC chuyên nghiệp. Trả về JSON theo schema đã định."),
            ChatMessage.CreateUserMessage(prompt),
        ]);

        try
        {
            var json = chatResult.Value.Content[0].Text;
            var dto = JsonSerializer.Deserialize<AIAnalysisDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new AIAnalysis
            {
                Id = Guid.NewGuid(),
                ResultId = result.Id,
                Summary = dto?.Summary ?? "",
                WeakParts = dto?.WeakParts?.Select(w => new WeakPoint(w.Part, w.Accuracy, w.PriorityLevel, w.Recommendation)).ToList() ?? [],
                StudyPlan = dto?.StudyPlan?.Select(p => new WeeklyPlan(p.Week, p.Focus, p.Tasks)).ToList() ?? [],
                DailyMinutes = dto?.DailyMinutes ?? 45,
                PredictedScore = dto?.PredictedScore ?? result.TotalScore,
                SuccessProbability = dto?.SuccessProbability ?? 50,
            };
        }
        catch
        {
            return new AIAnalysis { Id = Guid.NewGuid(), ResultId = result.Id, Summary = "Không thể phân tích." };
        }
    }

    private static string BuildPrompt(UserTestResult result) =>
        $$"""
        Phân tích kết quả TOEIC:
        - Điểm Listening: {{result.ListeningScaled}}/495
        - Điểm Reading: {{result.ReadingScaled}}/495
        - Tổng: {{result.TotalScore}}/990
        
        Trả về JSON với cấu trúc:
        {
          "summary": "Nhận xét ngắn",
          "weakParts": [{"part":5,"accuracy":45,"priorityLevel":"high","recommendation":"..."}],
          "studyPlan": [{"week":1,"focus":"Chủ đề tuần","tasks":["task1","task2"]}],
          "dailyMinutes": 45,
          "predictedScore": 750,
          "successProbability": 72
        }
        """;

    private record AIAnalysisDto(
        string Summary,
        List<WeakPointDto> WeakParts,
        List<WeeklyPlanDto> StudyPlan,
        int DailyMinutes,
        int PredictedScore,
        int SuccessProbability);

    private record WeakPointDto(int Part, int Accuracy, string PriorityLevel, string Recommendation);
    private record WeeklyPlanDto(int Week, string Focus, List<string> Tasks);
}
