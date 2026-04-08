using Npgsql;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Repositories; // só se precisar tipos concretos, senão remova

namespace StoreSyncBack.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return _repo.GetAllCategoriesAsync();
        }

        public Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return _repo.GetCategoryByIdAsync(categoryId);
        }

        public async Task<int> CreateCategoryAsync(Category category)
        {
            // validações simples
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Name é obrigatório", nameof(category.Name));

            // Força CreatedAt
            if (category.CreatedAt == default)
                category.CreatedAt = DateTime.UtcNow;

            try
            {
                return await _repo.CreateCategoryAsync(category);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException($"Já existe uma categoria com o nome '{category.Name}'.");
            }
        }

        public async Task<int> UpdateCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (category.CategoryId == Guid.Empty)
                throw new ArgumentException("CategoryId inválido", nameof(category.CategoryId));

            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Name é obrigatório", nameof(category.Name));

            try
            {
                return await _repo.UpdateCategoryAsync(category);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new InvalidOperationException($"Já existe uma categoria com o nome '{category.Name}'.");
            }
        }

        public Task<int> DeleteCategoryAsync(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId inválido", nameof(categoryId));

            return _repo.DeleteCategoryAsync(categoryId);
        }
    }
}
