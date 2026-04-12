using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _employeeRepoMock;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _employeeService = new EmployeeService(_employeeRepoMock.Object);
        }

        #region GetAllEmployeesAsync

        [Fact]
        public async Task GetAllEmployeesAsync_FuncionariosExistem_RetornaListaDeFuncionarios()
        {
            // Arrange
            var expectedEmployees = TestData.CreateEmployees(5);
            var paginated = new PaginatedResult<Employee> { Items = expectedEmployees, TotalCount = 5 };
            _employeeRepoMock.Setup(r => r.GetAllEmployeesAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginated);

            // Act
            var result = await _employeeService.GetAllEmployeesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
        }

        #endregion

        #region GetEmployeeByIdAsync

        [Fact]
        public async Task GetEmployeeByIdAsync_FuncionarioExistente_RetornaFuncionario()
        {
            // Arrange
            var expectedEmployee = TestData.CreateEmployee("admin");
            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(expectedEmployee.EmployeeId))
                .ReturnsAsync(expectedEmployee);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(expectedEmployee.EmployeeId);

            // Assert
            result.Should().NotBeNull();
            result!.EmployeeId.Should().Be(expectedEmployee.EmployeeId);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_FuncionarioInexistente_RetornaNull()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateEmployeeAsync

        [Fact]
        public async Task CreateEmployeeAsync_FuncionarioValido_RetornaIdCriado()
        {
            // Arrange
            var employee = TestData.CreateEmployee("user");
            _employeeRepoMock.Setup(r => r.CreateEmployeeAsync(It.IsAny<Employee>()))
                .ReturnsAsync(1);

            // Act
            var result = await _employeeService.CreateEmployeeAsync(employee);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task CreateEmployeeAsync_FuncionarioNull_LancaArgumentNullException()
        {
            // Arrange
            Employee? employee = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _employeeService.CreateEmployeeAsync(employee!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateEmployeeAsync_NomeInvalido_LancaArgumentException(string? name)
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            employee.Name = name ?? "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.CreateEmployeeAsync(employee));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateEmployeeAsync_CpfInvalido_LancaArgumentException(string? cpf)
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            employee.Cpf = cpf ?? "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.CreateEmployeeAsync(employee));
        }

        #endregion

        #region UpdateEmployeeAsync

        [Fact]
        public async Task UpdateEmployeeAsync_FuncionarioValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(It.IsAny<Employee>()))
                .ReturnsAsync(1);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_FuncionarioNull_LancaArgumentNullException()
        {
            // Arrange
            Employee? employee = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _employeeService.UpdateEmployeeAsync(employee!));
        }

        [Fact]
        public async Task UpdateEmployeeAsync_EmployeeIdVazio_LancaArgumentException()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            employee.EmployeeId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.UpdateEmployeeAsync(employee));
        }

        #endregion

        #region DeleteEmployeeAsync

        [Fact]
        public async Task DeleteEmployeeAsync_IdValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _employeeRepoMock.Setup(r => r.DeleteEmployeeAsync(employeeId))
                .ReturnsAsync(1);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(employeeId);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_IdVazio_LancaArgumentException()
        {
            // Arrange
            var employeeId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.DeleteEmployeeAsync(employeeId));
        }

        #endregion
    }
}
