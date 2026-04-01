using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IEmployeeRepository> _employeeRepoMock;
        private readonly Mock<IValidator<UserLoginDto>> _loginValidatorMock;
        private readonly Mock<IValidator<UserChangePasswordDto>> _changePasswordValidatorMock;
        private readonly UserService _userService;
        private readonly JwtSettings _jwtSettings;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _loginValidatorMock = new Mock<IValidator<UserLoginDto>>();
            _changePasswordValidatorMock = new Mock<IValidator<UserChangePasswordDto>>();

            _jwtSettings = new JwtSettings
            {
                Key = "esta-e-uma-chave-super-secreta-32-chars",
                Issuer = "StoreSyncBack",
                Audience = "StoreSyncBackClients",
                ExpiresMinutes = 60
            };

            var jwtOptionsMock = new Mock<IOptions<JwtSettings>>();
            jwtOptionsMock.Setup(o => o.Value).Returns(_jwtSettings);

            _userService = new UserService(
                _userRepoMock.Object,
                _employeeRepoMock.Object,
                jwtOptionsMock.Object,
                _loginValidatorMock.Object,
                _changePasswordValidatorMock.Object
            );
        }

        #region GetAllUsersAsync

        [Fact]
        public async Task GetAllUsersAsync_UsuariosExistem_RetornaListaDeUsuarios()
        {
            // Arrange
            var expectedUsers = TestData.CreateUsers(3);
            _userRepoMock.Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            _userRepoMock.Verify(r => r.GetAllUsersAsync(), Times.Once);
        }

        #endregion

        #region GetUserByIdAsync

        [Fact]
        public async Task GetUserByIdAsync_UsuarioExistente_RetornaUsuario()
        {
            // Arrange
            var expectedUser = TestData.CreateUser("testuser");
            _userRepoMock.Setup(r => r.GetUserByIdAsync(expectedUser.UserId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(expectedUser.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(expectedUser.UserId);
            result.Login.Should().Be("testuser");
        }

        [Fact]
        public async Task GetUserByIdAsync_UsuarioInexistente_RetornaNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Login

        [Fact]
        public async Task Login_CredenciaisValidas_RetornaUsuarioComToken()
        {
            // Arrange
            var loginDto = TestData.CreateUserLoginDto("admin", "admin123");
            var user = TestData.CreateUser("admin");
            user.Password = BCrypt.Net.BCrypt.HashPassword("admin123");
            var employee = TestData.CreateEmployee("admin");

            _loginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserLoginDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.Login(It.IsAny<UserLoginDto>()))
                .ReturnsAsync(user);
            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(employee);

            // Act
            var result = await _userService.Login(loginDto);

            // Assert
            result.Should().NotBeNull();
            result!.Login.Should().Be("admin");
            result.Token.Should().NotBeNullOrEmpty();
            result.Password.Should().BeNull();
        }

        [Fact]
        public async Task Login_DtoInvalido_LancaArgumentException()
        {
            // Arrange
            var loginDto = TestData.CreateUserLoginDto();
            var validationFailures = new List<ValidationFailure>
            {
                new("Login", "Login é obrigatório")
            };

            _loginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserLoginDto>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _userService.Login(loginDto));
        }

        [Fact]
        public async Task Login_UsuarioNaoEncontrado_RetornaNull()
        {
            // Arrange
            var loginDto = TestData.CreateUserLoginDto("naoexiste", "senha");

            _loginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserLoginDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.Login(It.IsAny<UserLoginDto>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.Login(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_SenhaIncorreta_RetornaNull()
        {
            // Arrange
            var loginDto = TestData.CreateUserLoginDto("admin", "senhaerrada");
            var user = TestData.CreateUser("admin");
            user.Password = BCrypt.Net.BCrypt.HashPassword("senhacerta");

            _loginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserLoginDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.Login(It.IsAny<UserLoginDto>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.Login(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_SenhaNullNoUsuario_RetornaNull()
        {
            // Arrange
            var loginDto = TestData.CreateUserLoginDto("admin", "admin");
            var user = TestData.CreateUser("admin");
            user.Password = null;

            _loginValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserLoginDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.Login(It.IsAny<UserLoginDto>()))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.Login(loginDto);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateUserAsync

        [Fact]
        public async Task CreateUserAsync_UsuarioValido_RetornaIdCriado()
        {
            // Arrange
            var user = TestData.CreateUser("novousuario", "senha123");
            _userRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            // Act
            var result = await _userService.CreateUserAsync(user);

            // Assert
            result.Should().Be(1);
            _userRepoMock.Verify(r => r.CreateUserAsync(It.Is<User>(
                u => u.Login == "novousuario")), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_UsuarioNull_LancaArgumentNullException()
        {
            // Arrange
            User? user = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _userService.CreateUserAsync(user!));
        }

        [Fact]
        public async Task CreateUserAsync_LoginVazio_LancaArgumentException()
        {
            // Arrange
            var user = TestData.CreateUser("", "senha123");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.CreateUserAsync(user));
        }

        [Fact]
        public async Task CreateUserAsync_SenhaVazia_LancaArgumentException()
        {
            // Arrange
            var user = TestData.CreateUser("usuario", "");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.CreateUserAsync(user));
        }

        [Fact]
        public async Task CreateUserAsync_SenhaEhHasheada()
        {
            // Arrange
            var plainPassword = "senha123";
            var user = TestData.CreateUser("usuario", plainPassword);
            string? capturedPassword = null;

            _userRepoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedPassword = u.Password)
                .ReturnsAsync(1);

            // Act
            await _userService.CreateUserAsync(user);

            // Assert
            capturedPassword.Should().NotBeNullOrEmpty();
            capturedPassword.Should().NotBe(plainPassword);
            BCrypt.Net.BCrypt.Verify(plainPassword, capturedPassword).Should().BeTrue();
        }

        #endregion

        #region UpdateUserAsync

        [Fact]
        public async Task UpdateUserAsync_UsuarioValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var user = TestData.CreateUser("usuario");
            user.UserId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            // Act
            var result = await _userService.UpdateUserAsync(user);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateUserAsync_UsuarioNull_LancaArgumentNullException()
        {
            // Arrange
            User? user = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _userService.UpdateUserAsync(user!));
        }

        [Fact]
        public async Task UpdateUserAsync_UserIdVazio_LancaArgumentException()
        {
            // Arrange
            var user = TestData.CreateUser("usuario");
            user.UserId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateUserAsync(user));
        }

        [Fact]
        public async Task UpdateUserAsync_ComSenha_SenhaEhHasheada()
        {
            // Arrange
            var plainPassword = "novasenha";
            var user = TestData.CreateUser("usuario", plainPassword);
            user.UserId = Guid.NewGuid();
            string? capturedPassword = null;

            _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .Callback<User>(u => capturedPassword = u.Password)
                .ReturnsAsync(1);

            // Act
            await _userService.UpdateUserAsync(user);

            // Assert
            capturedPassword.Should().NotBe(plainPassword);
            BCrypt.Net.BCrypt.Verify(plainPassword, capturedPassword!).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateUserAsync_SemSenha_NaoAlteraSenha()
        {
            // Arrange
            var user = TestData.CreateUser("usuario");
            user.UserId = Guid.NewGuid();
            user.Password = null;

            _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            // Act
            await _userService.UpdateUserAsync(user);

            // Assert
            _userRepoMock.Verify(r => r.UpdateUserAsync(It.Is<User>(
                u => u.Password == null)), Times.Once);
        }

        #endregion

        #region DeleteUserAsync

        [Fact]
        public async Task DeleteUserAsync_IdValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepoMock.Setup(r => r.DeleteUserAsync(userId))
                .ReturnsAsync(1);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteUserAsync_IdVazio_LancaArgumentException()
        {
            // Arrange
            var userId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.DeleteUserAsync(userId));
        }

        #endregion

        #region ChangeUserPasswordAsync

        [Fact]
        public async Task ChangeUserPasswordAsync_DadosValidos_RetornaTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = TestData.CreateUserChangePasswordDto(userId, "senhaantiga", "novasenha");
            var user = TestData.CreateUser("usuario", BCrypt.Net.BCrypt.HashPassword("senhaantiga"));
            user.UserId = userId;

            _changePasswordValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserChangePasswordDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            // Act
            var result = await _userService.ChangeUserPasswordAsync(dto);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_DtoInvalido_LancaArgumentException()
        {
            // Arrange
            var dto = TestData.CreateUserChangePasswordDto(Guid.NewGuid());
            var validationFailures = new List<ValidationFailure>
            {
                new("OldPassword", "Senha antiga é obrigatória")
            };

            _changePasswordValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserChangePasswordDto>(), default))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.ChangeUserPasswordAsync(dto));
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_UsuarioNaoEncontrado_LancaArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = TestData.CreateUserChangePasswordDto(userId);

            _changePasswordValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserChangePasswordDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.ChangeUserPasswordAsync(dto));
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_SenhaAntigaIncorreta_LancaArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = TestData.CreateUserChangePasswordDto(userId, "senhaerrada", "novasenha");
            var user = TestData.CreateUser("usuario", BCrypt.Net.BCrypt.HashPassword("senhacerta"));
            user.UserId = userId;

            _changePasswordValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserChangePasswordDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.ChangeUserPasswordAsync(dto));
            exception.Message.Should().Contain("Senha antiga inválida");
        }

        [Fact]
        public async Task ChangeUserPasswordAsync_SenhaAntigaNullNoUsuario_LancaArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = TestData.CreateUserChangePasswordDto(userId, "qualquer", "novasenha");
            var user = TestData.CreateUser("usuario");
            user.UserId = userId;
            user.Password = null;

            _changePasswordValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UserChangePasswordDto>(), default))
                .ReturnsAsync(new ValidationResult());
            _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.ChangeUserPasswordAsync(dto));
        }

        #endregion
    }
}
