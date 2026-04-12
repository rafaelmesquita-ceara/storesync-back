namespace SharedModels.Interfaces;

public interface IUserRepository
{
    Task<PaginatedResult<User>> GetAllUsersAsync(int limit = 50, int offset = 0);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> Login(UserLoginDto userLoginDto);
    Task<int> CreateUserAsync(User user);
    Task<int> UpdateUserAsync(User user);
    Task<int> DeleteUserAsync(Guid userId);
}

public interface IUserService
{
    Task<PaginatedResult<User>> GetAllUsersAsync(int limit = 50, int offset = 0);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> Login(UserLoginDto userLoginDto);
    Task<int> CreateUserAsync(User user);
    Task<int> UpdateUserAsync(User user);
    Task<int> DeleteUserAsync(Guid userId);
    Task<bool> ChangeUserPasswordAsync(UserChangePasswordDto userLoginDto);
}