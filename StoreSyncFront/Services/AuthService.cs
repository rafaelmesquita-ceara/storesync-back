using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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
    
    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<User?> Login(UserLoginDto userLoginDto)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> Login()
    {
        throw new NotImplementedException();
    }

    public User? GetLoggedUser()
    {
        return _loggedUser;
    }

    public Task<int> CreateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeUserPasswordAsync(UserChangePasswordDto userLoginDto)
    {
        throw new NotImplementedException();
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
                _loggedUser = user;
                _apiService.SetApiKey(user.Token);
                return true;
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
        // 1. Limpa o usuário em memória
        _loggedUser = null;
        
        // 2. Remove a chave de API do serviço
        _apiService.SetApiKey(null);

        // 3. Deleta o arquivo de sessão para impedir o login automático
        if (File.Exists(UserDataFile))
        {
            File.Delete(UserDataFile);
        }
    }
}