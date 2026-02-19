using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Services;

public interface IQuestionService
{
    Task<List<Question>> GetQuestionsByCategoryAsync(int categoryId);
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<Question> CreateQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(int id);
    Task<List<Question>> ImportQuestionsFromJsonAsync(Stream jsonStream, int categoryId, string userId, string userEmail);
    Task<List<Question>> GetRandomQuestionsAsync(int categoryId, int? count = null);
    Task FlagQuestionAsync(int questionId, string userId, string userEmail, string? comments);
    Task<List<QuestionFlag>> GetFlaggedQuestionsAsync(bool includeResolved = false);
    Task ResolveFlagAsync(int flagId, string userId);
}
