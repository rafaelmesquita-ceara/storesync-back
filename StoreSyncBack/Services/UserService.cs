using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedModels;
using SharedModels.Interfaces;
using FluentValidation;

namespace StoreSyncBack.Services
{
    public class JwtSettings
    {
        public string Key { get; set; } = "";
        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public int ExpiresMinutes { get; set; } = 60;
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly JwtSettings _jwtSettings;
        private readonly IValidator<UserLoginDto> _loginValidator;
        private readonly IValidator<UserChangePasswordDto> _changePasswordValidator;

        public UserService(
            IUserRepository repo,
            IEmployeeRepository employeeRepo,
            IOptions<JwtSettings> jwtOptions,
            IValidator<UserLoginDto> loginValidator,
            IValidator<UserChangePasswordDto> changePasswordValidator)
        {
            _repo = repo;
            _employeeRepo = employeeRepo;
            _jwtSettings = jwtOptions.Value;
            _loginValidator = loginValidator;
            _changePasswordValidator = changePasswordValidator;
        }

        public Task<IEnumerable<User>> GetAllUsersAsync() => _repo.GetAllUsersAsync();

        public Task<User?> GetUserByIdAsync(Guid userId) => _repo.GetUserByIdAsync(userId);

        public async Task<User?> Login(UserLoginDto userLoginDto)
        {
            var validation = await _loginValidator.ValidateAsync(userLoginDto);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

            var user = await _repo.Login(userLoginDto);
            if (user == null)
                return null;

            // Verifica senha
            if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(userLoginDto.Password ?? "", user.Password))
                return null;

            // Gera token JWT
            user.Token = GenerateJwtToken(user);
            // Por segurança, opcional: não retornar a senha
            user.Password = null;

            user.Employee = await _employeeRepo.GetEmployeeByIdAsync(user.EmployeeId!.Value);

            return user;
        }

        public async Task<int> CreateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Login)) throw new ArgumentException("Login é obrigatório.", nameof(user.Login));
            if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentException("Password é obrigatório.", nameof(user.Password));

            // Hashear a senha antes de salvar
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            return await _repo.CreateUserAsync(user);
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.UserId == Guid.Empty) throw new ArgumentException("UserId inválido.", nameof(user.UserId));

            var existing = await _repo.GetUserByIdAsync(user.UserId);
            if (existing == null) throw new ArgumentException("Usuário não encontrado.");

            if (string.IsNullOrWhiteSpace(user.Password))
                user.Password = existing.Password;
            else
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            if (!user.EmployeeId.HasValue || user.EmployeeId == Guid.Empty)
                user.EmployeeId = existing.EmployeeId;

            return await _repo.UpdateUserAsync(user);
        }

        public Task<int> DeleteUserAsync(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId inválido.", nameof(userId));
            return _repo.DeleteUserAsync(userId);
        }

        public async Task<bool> ChangeUserPasswordAsync(UserChangePasswordDto dto)
        {
            var validation = await _changePasswordValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

            var user = await _repo.GetUserByIdAsync(dto.UserId);
            if (user == null)
                throw new ArgumentException("Usuário não encontrado.");

            if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(dto.OldPassword ?? "", user.Password))
                throw new ArgumentException("Senha antiga inválida.");

            // atualiza senha
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            var affected = await _repo.UpdateUserAsync(user);
            return affected > 0;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Login ?? ""),
                new Claim(ClaimTypes.Role, user.Employee?.Role ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
