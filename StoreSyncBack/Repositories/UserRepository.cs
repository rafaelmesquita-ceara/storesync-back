using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _db;

        public UserRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var sql = @"SELECT user_id AS UserId, login AS Login, password AS Password, employee_id AS EmployeeId
                        FROM ""user"" -- se você usou nome reservado, ajuste
                        ORDER BY login;";
            return await _db.QueryAsync<User>(sql);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            var sql = @"SELECT user_id AS UserId, login AS Login, password AS Password, employee_id AS EmployeeId
                        FROM ""user""
                        WHERE user_id = @Id;";
            return await _db.QueryFirstOrDefaultAsync<User?>(sql, new { Id = userId });
        }

        public async Task<User?> Login(UserLoginDto userLoginDto)
        {
            var sql = @"SELECT user_id AS UserId, login AS Login, password AS Password, employee_id AS EmployeeId
                        FROM ""user""
                        WHERE login = @Login;";
            return await _db.QueryFirstOrDefaultAsync<User?>(sql, new { Login = userLoginDto.Login });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            if (user.UserId == Guid.Empty)
                user.UserId = Guid.NewGuid();

            var sql = @"
                INSERT INTO ""user"" (user_id, login, password, employee_id)
                VALUES (@UserId, @Login, @Password, @EmployeeId);
            ";
            var affected = await _db.ExecuteAsync(sql, new
            {
                user.UserId,
                user.Login,
                user.Password,
                EmployeeId = user.EmployeeId
            });
            return affected;
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            var sql = @"
                UPDATE ""user""
                SET login = @Login, password = @Password, employee_id = @EmployeeId
                WHERE user_id = @UserId;
            ";
            var affected = await _db.ExecuteAsync(sql, new
            {
                user.Login,
                user.Password,
                EmployeeId = user.EmployeeId,
                user.UserId
            });
            return affected;
        }

        public async Task<int> DeleteUserAsync(Guid userId)
        {
            var sql = @"DELETE FROM ""user"" WHERE user_id = @Id;";
            var affected = await _db.ExecuteAsync(sql, new { Id = userId });
            return affected;
        }
    }
}
