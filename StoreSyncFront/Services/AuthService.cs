using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace StoreSyncFront.Services;

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private User? _loggedUser;
    private const string UserDataFile = "user_data.json";

    public AuthService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<PaginatedResult<User>> GetAllUsersAsync(int limit = 50, int offset = 0)
    {
        Response response = await _apiService.GetAsync($"/api/Users?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<User>>(response.Body) ?? new PaginatedResult<User>();

        SnackBarService.SendError("Erro ao buscar usuários: " + response.Body);
        return new PaginatedResult<User> { Items = new List<User>() };
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        Response response = await _apiService.GetAsync($"/api/Users/{userId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<User>(response.Body);

        SnackBarService.SendError("Erro ao buscar usuário: " + response.Body);
        return null;
    }

    public async Task<User?> Login(UserLoginDto userLoginDto)
    {
        var content = JsonContent.Create(userLoginDto);
        Response response = await _apiService.PostAsync("/api/Users/login", content);
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<User>(response.Body);

        return null;
    }

    public User? GetLoggedUser()
    {
        return _loggedUser;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        Response response = await _apiService.PostAsync("/api/Users", JsonContent.Create(user));
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Usuário cadastrado com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao cadastrar usuário: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        Response response = await _apiService.PutAsync($"/api/Users/{user.UserId}", JsonContent.Create(user));
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Usuário atualizado com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao atualizar usuário: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> DeleteUserAsync(Guid userId)
    {
        Response response = await _apiService.DeleteAsync($"/api/Users/{userId}");
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Usuário excluído com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao excluir usuário: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<bool> ChangeUserPasswordAsync(UserChangePasswordDto dto)
    {
        Response response = await _apiService.PostAsync("/api/Users/change-password", JsonContent.Create(dto));
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Senha alterada com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao alterar senha: " + response.Body);
        return response.IsSuccess();
    }

    public async Task<string> Auth(UserLoginDto userLoginDto)
    {
        try
        {
            var content = JsonContent.Create(userLoginDto);
            Response response = await _apiService.PostAsync("/api/Users/login", content);
            if (response.IsSuccess())
            {
                User user = JsonConvert.DeserializeObject<User>(response.Body);

                _apiService.SetApiKey(user.Token);
                _loggedUser = user;
                await SaveUserDataAsync(user);
                return string.Empty;
            }

            return response.Body;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Falha na autenticação: {ex.Message}");
            return "Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.";
        }
    }

    private async Task SaveUserDataAsync(User user)
    {
        try
        {
            string json = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(UserDataFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Erro ao salvar dados do usuário: {ex.Message}");
        }
    }

    public async Task<bool> LoadUserDataAsync()
    {
        try
        {
            if (!File.Exists(UserDataFile))
                return false;

            string json = await File.ReadAllTextAsync(UserDataFile);
            User user = JsonSerializer.Deserialize<User>(json);

            if (user != null && !string.IsNullOrEmpty(user.Token))
            {
                _apiService.SetApiKey(user.Token);
                
                // Verify if token is still valid
                var response = await _apiService.GetAsync($"/api/Users/{user.UserId}");
                
                if (response.IsSuccess())
                {
                    _loggedUser = user;
                    return true;
                }
                
                if (response.Status == 401 || response.Status == 403)
                {
                    Console.WriteLine("[AuthService] Token expirado ou inválido. Limpando credenciais locais.");
                    Logout();
                }

                return false;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AuthService] Erro ao carregar dados do usuário: {ex.Message}");
            return false;
        }
    }

    public void Logout()
    {
        _loggedUser = null;
        _apiService.SetApiKey(null);

        if (File.Exists(UserDataFile))
            File.Delete(UserDataFile);
    }
}
