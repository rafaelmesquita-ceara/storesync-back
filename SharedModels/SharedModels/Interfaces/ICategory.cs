namespace SharedModels.Interfaces;

public interface ICategoryRepository
{
    Task<PaginatedResult<Category>> GetAllCategoriesAsync(int limit = 50, int offset = 0);
    Task<Category?> GetCategoryByIdAsync(Guid categoryId);
    Task<int> CreateCategoryAsync(Category category);
    Task<int> UpdateCategoryAsync(Category category);
    Task<int> DeleteCategoryAsync(Guid categoryId);
}

public interface ICategoryService
{
    Task<PaginatedResult<Category>> GetAllCategoriesAsync(int limit = 50, int offset = 0);
    Task<Category?> GetCategoryByIdAsync(Guid categoryId);
    Task<int> CreateCategoryAsync(Category category);
    Task<int> UpdateCategoryAsync(Category category);
    Task<int> DeleteCategoryAsync(Guid categoryId);
}