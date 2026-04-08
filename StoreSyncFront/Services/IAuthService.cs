using System.Threading.Tasks;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncFront.Services;

public interface IAuthService : IUserService
{
    Task<string> Auth(UserLoginDto userLoginDto);
    Task<bool> LoadUserDataAsync();
    User GetLoggedUser();
    void Logout();
}