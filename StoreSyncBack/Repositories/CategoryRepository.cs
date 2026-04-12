using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnection _db;

        public CategoryRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PaginatedResult<Category>> GetAllCategoriesAsync(int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM category;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql);

            var sql = @"SELECT category_id AS CategoryId, name AS Name, created_at AS CreatedAt
                        FROM category
                        ORDER BY name
                        LIMIT @Limit OFFSET @Offset;";
            
            var result = await _db.QueryAsync<Category>(sql, new { Limit = limit, Offset = offset });

            return new PaginatedResult<Category>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            var sql = @"SELECT category_id AS CategoryId, name AS Name, created_at AS CreatedAt
                        FROM category
                        WHERE category_id = @Id;";
            return await _db.QueryFirstOrDefaultAsync<Category?>(sql, new { Id = categoryId });
        }

        public async Task<int> CreateCategoryAsync(Category category)
        {
            if (category.CategoryId == Guid.Empty)
                category.CategoryId = Guid.NewGuid();

            if (category.CreatedAt == default)
                category.CreatedAt = BrazilDateTime.Now;

            var sql = @"
                INSERT INTO category (category_id, name, created_at)
                VALUES (@CategoryId, @Name, @CreatedAt);";

            var affected = await _db.ExecuteAsync(sql, category);
            return affected; // normalmente 1 se inserido
        }

        public async Task<int> UpdateCategoryAsync(Category category)
        {
            var sql = @"
                UPDATE category
                SET name = @Name
                WHERE category_id = @CategoryId;";
            var affected = await _db.ExecuteAsync(sql, new { category.Name, category.CategoryId });
            return affected; // linhas afetadas (0 se não existia)
        }

        public async Task<int> DeleteCategoryAsync(Guid categoryId)
        {
            var sql = "DELETE FROM category WHERE category_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = categoryId });
            return affected;
        }
    }
}