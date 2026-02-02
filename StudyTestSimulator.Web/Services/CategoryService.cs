using Microsoft.EntityFrameworkCore;
using StudyTestSimulator.Web.Data;
using StudyTestSimulator.Web.Models;

namespace StudyTestSimulator.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestCategory>> GetAllCategoriesAsync()
    {
        return await _context.TestCategories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<TestCategory?> GetCategoryByIdAsync(int id)
    {
        return await _context.TestCategories
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<TestCategory> CreateCategoryAsync(TestCategory category)
    {
        category.CreatedDate = DateTime.UtcNow;
        _context.TestCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(TestCategory category)
    {
        category.ModifiedDate = DateTime.UtcNow;
        _context.TestCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.TestCategories.FindAsync(id);
        if (category != null)
        {
            _context.TestCategories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetQuestionCountAsync(int categoryId)
    {
        return await _context.Questions
            .Where(q => q.TestCategoryId == categoryId && q.IsActive)
            .CountAsync();
    }
}
