using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Services;

public interface ICategoryService
{
    Task<List<TestCategory>> GetAllCategoriesAsync();
    Task<TestCategory?> GetCategoryByIdAsync(int id);
    Task<TestCategory> CreateCategoryAsync(TestCategory category);
    Task UpdateCategoryAsync(TestCategory category);
    Task DeleteCategoryAsync(int id);
    Task<int> GetQuestionCountAsync(int categoryId);
}
