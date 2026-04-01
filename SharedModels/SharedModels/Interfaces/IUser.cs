namespace SharedModels.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> Login(UserLoginDto userLoginDto);
    Task<int> CreateUserAsync(User user);
    Task<int> UpdateUserAsync(User user);
    Task<int> DeleteUserAsync(Guid userId);
}

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> Login(UserLoginDto userLoginDto);
    Task<int> CreateUserAsync(User user);
    Task<int> UpdateUserAsync(User user);
    Task<int> DeleteUserAsync(Guid userId);
    Task<bool> ChangeUserPasswordAsync(UserChangePasswordDto userLoginDto);
}