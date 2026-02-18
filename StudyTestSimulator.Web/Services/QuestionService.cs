using Microsoft.EntityFrameworkCore;
using StudyTestSimulator.Web.Data;
using StudyTestSimulator.Web.Models;
using StudyTestSimulator.Web.DTOs;
using System.Text.Json;

namespace StudyTestSimulator.Web.Services;

public class QuestionService : IQuestionService
{
    private readonly ApplicationDbContext _context;

    public QuestionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Question>> GetQuestionsByCategoryAsync(int categoryId)
    {
        return await _context.Questions
            .Include(q => q.Answers.OrderBy(a => a.DisplayOrder))
            .Include(q => q.Flags.Where(f => !f.IsResolved))
            .Where(q => q.TestCategoryId == categoryId)
            .OrderByDescending(q => q.CreatedDate)
            .ToListAsync();
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await _context.Questions
            .Include(q => q.Answers.OrderBy(a => a.DisplayOrder))
            .Include(q => q.TestCategory)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        question.CreatedDate = DateTime.UtcNow;
        question.IsActive = true;
        
        // Set display order for answers
        for (int i = 0; i < question.Answers.Count; i++)
        {
            question.Answers.ElementAt(i).DisplayOrder = i;
        }

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        var existing = await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == question.Id);

        if (existing == null) return;

        // Update scalar properties on the tracked entity
        existing.QuestionText = question.QuestionText;
        existing.ImageBase64 = question.ImageBase64;
        existing.Explanation = question.Explanation;
        existing.ModifiedDate = DateTime.UtcNow;
        existing.ModifiedBy = question.ModifiedBy;

        // Replace answers: remove old, add new
        _context.Answers.RemoveRange(existing.Answers);
        foreach (var answer in question.Answers)
        {
            existing.Answers.Add(new Answer
            {
                AnswerText = answer.AnswerText,
                IsCorrect = answer.IsCorrect,
                Explanation = answer.Explanation,
                DisplayOrder = answer.DisplayOrder
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            question.IsActive = false;
            question.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Question>> ImportQuestionsFromJsonAsync(string json, int categoryId, string userId, string userEmail)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var importData = JsonSerializer.Deserialize<QuestionImportDto>(json, options);
        if (importData?.Questions == null || !importData.Questions.Any())
        {
            throw new ArgumentException("Invalid JSON format or no questions found.");
        }

        var questions = new List<Question>();

        foreach (var qDto in importData.Questions)
        {
            if (qDto.Answers == null || qDto.Answers.Count < 2)
            {
                throw new ArgumentException($"Question '{qDto.QuestionText}' must have at least 2 answers.");
            }

            if (!qDto.Answers.Any(a => a.IsCorrect))
            {
                throw new ArgumentException($"Question '{qDto.QuestionText}' must have at least one correct answer.");
            }

            var question = new Question
            {
                TestCategoryId = categoryId,
                QuestionText = qDto.QuestionText,
                ImageBase64 = qDto.ImageBase64,
                Explanation = qDto.Explanation,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            for (int i = 0; i < qDto.Answers.Count; i++)
            {
                var ansDto = qDto.Answers[i];
                question.Answers.Add(new Answer
                {
                    AnswerText = ansDto.AnswerText,
                    IsCorrect = ansDto.IsCorrect,
                    Explanation = ansDto.Explanation,
                    DisplayOrder = i
                });
            }

            questions.Add(question);
        }

        _context.Questions.AddRange(questions);
        await _context.SaveChangesAsync();

        return questions;
    }

    public async Task<List<Question>> GetRandomQuestionsAsync(int categoryId, int? count = null)
    {
        var query = _context.Questions
            .Include(q => q.Answers.OrderBy(a => a.DisplayOrder))
            .Where(q => q.TestCategoryId == categoryId && q.IsActive);

        var totalQuestions = await query.CountAsync();
        
        if (totalQuestions == 0)
        {
            return new List<Question>();
        }

        var questionCount = count ?? totalQuestions;
        
        // Get random questions using OrderBy(Guid.NewGuid())
        return await query
            .OrderBy(q => Guid.NewGuid())
            .Take(questionCount)
            .ToListAsync();
    }

    public async Task FlagQuestionAsync(int questionId, string userId, string userEmail, string? comments)
    {
        var flag = new QuestionFlag
        {
            QuestionId = questionId,
            FlaggedBy = userId,
            FlaggedByEmail = userEmail,
            FlaggedDate = DateTime.UtcNow,
            Comments = comments,
            IsResolved = false
        };

        _context.QuestionFlags.Add(flag);
        await _context.SaveChangesAsync();
    }

    public async Task<List<QuestionFlag>> GetFlaggedQuestionsAsync(bool includeResolved = false)
    {
        var query = _context.QuestionFlags
            .Include(f => f.Question)
                .ThenInclude(q => q.TestCategory)
            .AsQueryable();

        if (!includeResolved)
        {
            query = query.Where(f => !f.IsResolved);
        }

        return await query
            .OrderByDescending(f => f.FlaggedDate)
            .ToListAsync();
    }

    public async Task ResolveFlagAsync(int flagId, string userId)
    {
        var flag = await _context.QuestionFlags.FindAsync(flagId);
        if (flag != null)
        {
            flag.IsResolved = true;
            flag.ResolvedBy = userId;
            flag.ResolvedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
