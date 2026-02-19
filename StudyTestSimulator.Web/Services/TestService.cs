using Microsoft.EntityFrameworkCore;
using StudyTestSimulator.Web.Data;
using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Services;

public class TestService : ITestService
{
    private readonly ApplicationDbContext _context;
    private readonly IQuestionService _questionService;

    public TestService(ApplicationDbContext context, IQuestionService questionService)
    {
        _context = context;
        _questionService = questionService;
    }

    public async Task<TestAttempt> StartTestAsync(int categoryId, string userId, string userEmail, int? questionCount = null)
    {
        // Check if user has an active test
        var activeTest = await GetActiveTestAsync(userId);
        if (activeTest != null)
        {
            throw new InvalidOperationException("You already have an active test. Please complete or cancel it before starting a new one.");
        }

        // Get random questions
        var questions = await _questionService.GetRandomQuestionsAsync(categoryId, questionCount);
        
        if (!questions.Any())
        {
            throw new InvalidOperationException("No questions available for this category.");
        }

        // Create test attempt
        var attempt = new TestAttempt
        {
            TestCategoryId = categoryId,
            UserId = userId,
            UserEmail = userEmail,
            StartTime = DateTime.UtcNow,
            TotalQuestions = questions.Count,
            CorrectAnswers = 0,
            PercentageScore = 0,
            IsCompleted = false
        };

        _context.TestAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Create test attempt questions with random order
        for (int i = 0; i < questions.Count; i++)
        {
            var attemptQuestion = new TestAttemptQuestion
            {
                TestAttemptId = attempt.Id,
                QuestionId = questions[i].Id,
                QuestionOrder = i,
                QuestionStartTime = DateTime.UtcNow // Will be updated when user starts answering
            };
            _context.TestAttemptQuestions.Add(attemptQuestion);
        }

        await _context.SaveChangesAsync();

        // Reload with all navigation properties
        return (await GetTestAttemptDetailsAsync(attempt.Id))!;
    }

    public async Task<TestAttempt?> GetActiveTestAsync(string userId)
    {
        return await _context.TestAttempts
            .Include(t => t.TestCategory)
            .Include(t => t.TestAttemptQuestions)
                .ThenInclude(taq => taq.Question)
                    .ThenInclude(q => q.Answers.OrderBy(a => a.DisplayOrder))
            .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsCompleted);
    }

    public async Task SubmitAnswerAsync(int attemptId, int questionId, int? answerId, int timeSpentSeconds)
    {
        var attemptQuestion = await _context.TestAttemptQuestions
            .Include(taq => taq.Question)
                .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(taq => taq.TestAttemptId == attemptId && taq.QuestionId == questionId);

        if (attemptQuestion == null)
        {
            throw new ArgumentException("Question not found in this test attempt.");
        }

        // Update attempt question
        attemptQuestion.SelectedAnswerId = answerId;
        attemptQuestion.QuestionEndTime = DateTime.UtcNow;
        attemptQuestion.TimeSpentSeconds = timeSpentSeconds;

        // Check if answer is correct
        if (answerId.HasValue)
        {
            var selectedAnswer = attemptQuestion.Question.Answers.FirstOrDefault(a => a.Id == answerId.Value);
            attemptQuestion.IsCorrect = selectedAnswer?.IsCorrect ?? false;
        }
        else
        {
            attemptQuestion.IsCorrect = false;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> CheckAnswerAsync(int questionId, int answerId)
    {
        var answer = await _context.Answers
            .FirstOrDefaultAsync(a => a.Id == answerId && a.QuestionId == questionId);

        return answer?.IsCorrect ?? false;
    }

    public async Task<TestAttempt> CompleteTestAsync(int attemptId)
    {
        var attempt = await _context.TestAttempts
            .Include(t => t.TestAttemptQuestions)
            .FirstOrDefaultAsync(t => t.Id == attemptId);

        if (attempt == null)
        {
            throw new ArgumentException("Test attempt not found.");
        }

        if (attempt.IsCompleted)
        {
            throw new InvalidOperationException("This test has already been completed.");
        }

        // Mark unanswered questions as skipped
        foreach (var q in attempt.TestAttemptQuestions.Where(q => q.SelectedAnswerId == null))
        {
            q.IsSkipped = true;
        }

        // Calculate score
        attempt.CorrectAnswers = attempt.TestAttemptQuestions.Count(q => q.IsCorrect == true);
        attempt.SkippedQuestions = attempt.TestAttemptQuestions.Count(q => q.IsSkipped);
        attempt.PercentageScore = attempt.TotalQuestions > 0
            ? Math.Round((decimal)attempt.CorrectAnswers / attempt.TotalQuestions * 100, 2)
            : 0;
        attempt.EndTime = DateTime.UtcNow;
        attempt.IsCompleted = true;

        await _context.SaveChangesAsync();

        return (await GetTestAttemptDetailsAsync(attemptId))!;
    }

    public async Task<TestAttempt?> AbandonTestAsync(int attemptId)
    {
        var attempt = await _context.TestAttempts
            .Include(t => t.TestAttemptQuestions)
            .FirstOrDefaultAsync(t => t.Id == attemptId);

        if (attempt == null)
            throw new ArgumentException("Test attempt not found.");

        if (attempt.IsCompleted)
            throw new InvalidOperationException("This test has already been completed.");

        bool hasAnsweredQuestions = attempt.TestAttemptQuestions
            .Any(q => q.SelectedAnswerId != null);

        if (!hasAnsweredQuestions)
        {
            // No answers given -- delete the entire attempt (cascade deletes children)
            _context.TestAttempts.Remove(attempt);
            await _context.SaveChangesAsync();
            return null;
        }

        // Has at least one answered question -- mark unanswered as skipped and complete
        var now = DateTime.UtcNow;
        foreach (var question in attempt.TestAttemptQuestions)
        {
            if (question.SelectedAnswerId == null)
            {
                question.IsSkipped = true;
                question.IsCorrect = false;
                question.QuestionEndTime = now;
            }
        }

        attempt.CorrectAnswers = attempt.TestAttemptQuestions.Count(q => q.IsCorrect);
        attempt.SkippedQuestions = attempt.TestAttemptQuestions.Count(q => q.IsSkipped);
        attempt.PercentageScore = attempt.TotalQuestions > 0
            ? Math.Round((decimal)attempt.CorrectAnswers / attempt.TotalQuestions * 100, 2)
            : 0;
        attempt.EndTime = now;
        attempt.IsCompleted = true;
        attempt.WasAbandoned = true;

        await _context.SaveChangesAsync();

        return (await GetTestAttemptDetailsAsync(attemptId))!;
    }

    public async Task<List<TestAttempt>> GetTestHistoryAsync(string userId, int? categoryId = null)
    {
        var query = _context.TestAttempts
            .Include(t => t.TestCategory)
            .Where(t => t.UserId == userId && t.IsCompleted);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.TestCategoryId == categoryId.Value);
        }

        return await query
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<TestAttempt?> GetTestAttemptDetailsAsync(int attemptId)
    {
        return await _context.TestAttempts
            .Include(t => t.TestCategory)
            .Include(t => t.TestAttemptQuestions.OrderBy(taq => taq.QuestionOrder))
                .ThenInclude(taq => taq.Question)
                    .ThenInclude(q => q.Answers.OrderBy(a => a.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == attemptId);
    }

    public async Task<List<TestAttempt>> GetRecentAttemptsAsync(int count = 10)
    {
        return await _context.TestAttempts
            .Include(t => t.TestCategory)
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.StartTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<TestAttempt?> GetLastAttemptForCategoryAsync(string userId, int categoryId)
    {
        return await _context.TestAttempts
            .Include(t => t.TestCategory)
            .Where(t => t.UserId == userId && t.TestCategoryId == categoryId && t.IsCompleted)
            .OrderByDescending(t => t.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<(List<TestAttempt> Items, int TotalCount)> GetAllTestHistoryPagedAsync(int? categoryId, int page, int pageSize)
    {
        var query = _context.TestAttempts
            .Include(t => t.TestCategory)
            .Where(t => t.IsCompleted);

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(t => t.TestCategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
