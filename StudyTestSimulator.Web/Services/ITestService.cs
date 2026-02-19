using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Services;

public interface ITestService
{
    Task<TestAttempt> StartTestAsync(int categoryId, string userId, string userEmail, int? questionCount = null);
    Task<TestAttempt?> GetActiveTestAsync(string userId);
    Task SubmitAnswerAsync(int attemptId, int questionId, int? answerId, int timeSpentSeconds);
    Task<bool> CheckAnswerAsync(int questionId, int answerId);
    Task<TestAttempt> CompleteTestAsync(int attemptId);
    Task<TestAttempt?> AbandonTestAsync(int attemptId);
    Task<List<TestAttempt>> GetTestHistoryAsync(string userId, int? categoryId = null);
    Task<TestAttempt?> GetTestAttemptDetailsAsync(int attemptId);
    Task<List<TestAttempt>> GetRecentAttemptsAsync(int count = 10);
    Task<TestAttempt?> GetLastAttemptForCategoryAsync(string userId, int categoryId);
    Task<(List<TestAttempt> Items, int TotalCount)> GetAllTestHistoryPagedAsync(int? categoryId, int page, int pageSize);
}
