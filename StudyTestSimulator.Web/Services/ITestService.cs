using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Services;

public interface ITestService
{
    Task<TestAttempt> StartTestAsync(int categoryId, string userId, string userEmail, int? questionCount = null);
    Task<TestAttempt?> GetActiveTestAsync(string userId);
    Task SubmitAnswerAsync(int attemptId, int questionId, int? answerId, int timeSpentSeconds);
    Task<bool> CheckAnswerAsync(int questionId, int answerId);
    Task<TestAttempt> CompleteTestAsync(int attemptId);
    Task<List<TestAttempt>> GetTestHistoryAsync(string userId, int? categoryId = null);
    Task<TestAttempt?> GetTestAttemptDetailsAsync(int attemptId);
}
