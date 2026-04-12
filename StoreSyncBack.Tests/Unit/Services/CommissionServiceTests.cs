using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class CommissionServiceTests
    {
        private readonly Mock<ICommissionRepository> _repoMock;
        private readonly Mock<ISaleRepository> _saleRepoMock;
        private readonly Mock<IEmployeeRepository> _employeeRepoMock;
        private readonly CommissionService _service;

        public CommissionServiceTests()
        {
            _repoMock = new Mock<ICommissionRepository>();
            _saleRepoMock = new Mock<ISaleRepository>();
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _service = new CommissionService(_repoMock.Object, _saleRepoMock.Object, _employeeRepoMock.Object);
        }

        #region CalculateAsync

        [Fact]
        public async Task CalculateAsync_PeriodoComVendas_RetornaValoresCorretos()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            employee.CommissionRate = 10m;
            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 1, 31);

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId))
                .ReturnsAsync(employee);
            _saleRepoMock.Setup(r => r.GetTotalSalesByEmployeeAndPeriodAsync(employee.EmployeeId, startDate, endDate))
                .ReturnsAsync(1000m);

            // Act
            var (totalSales, commissionRate, commissionValue) = await _service.CalculateAsync(employee.EmployeeId, startDate, endDate);

            // Assert
            totalSales.Should().Be(1000m);
            commissionRate.Should().Be(10m);
            commissionValue.Should().Be(100m); // 1000 * 10 / 100
        }

        [Fact]
        public async Task CalculateAsync_FuncionarioInexistente_LancaArgumentException()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CalculateAsync(employeeId, DateTime.Today.AddDays(-30), DateTime.Today));
        }

        #endregion

        #region CreateCommissionAsync

        [Fact]
        public async Task CreateCommissionAsync_DataInicialMaiorQueFinal_LancaArgumentException()
        {
            // Arrange
            var commission = TestData.CreateCommission();
            commission.StartDate = new DateTime(2025, 2, 1);
            commission.EndDate = new DateTime(2025, 1, 1); // anterior à start

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateCommissionAsync(commission));

            ex.Message.Should().Contain("Data inicial");
        }

        [Fact]
        public async Task CreateCommissionAsync_PeriodoSobrepostoExistente_LancaInvalidOperationException()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            var commission = TestData.CreateCommission(employeeId: employee.EmployeeId);
            commission.StartDate = new DateTime(2025, 1, 1);
            commission.EndDate = new DateTime(2025, 1, 31);

            var existing = TestData.CreateCommission(reference: "001", employeeId: employee.EmployeeId);
            existing.StartDate = new DateTime(2025, 1, 15);
            existing.EndDate = new DateTime(2025, 2, 15);

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId))
                .ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetOverlappingCommissionAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync(existing);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateCommissionAsync(commission));

            ex.Message.Should().Contain("001");
        }

        [Fact]
        public async Task CreateCommissionAsync_SemVendasNoPeriodo_LancaInvalidOperationException()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            var commission = TestData.CreateCommission(employeeId: employee.EmployeeId);
            commission.StartDate = new DateTime(2025, 1, 1);
            commission.EndDate = new DateTime(2025, 1, 31);

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId))
                .ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetOverlappingCommissionAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync((Commission?)null);
            _saleRepoMock.Setup(r => r.GetTotalSalesByEmployeeAndPeriodAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync(0m);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateCommissionAsync(commission));

            ex.Message.Should().Contain("Nenhuma venda");
        }

        [Fact]
        public async Task CreateCommissionAsync_DadosValidos_SnapshotaTaxaDoFuncionario()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            employee.CommissionRate = 7.5m;
            var commission = TestData.CreateCommission(employeeId: employee.EmployeeId);
            commission.StartDate = new DateTime(2025, 1, 1);
            commission.EndDate = new DateTime(2025, 1, 31);

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId))
                .ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetOverlappingCommissionAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync((Commission?)null);
            _saleRepoMock.Setup(r => r.GetTotalSalesByEmployeeAndPeriodAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync(2000m);
            _repoMock.Setup(r => r.CreateCommissionAsync(It.IsAny<Commission>()))
                .ReturnsAsync(1);

            // Act
            await _service.CreateCommissionAsync(commission);

            // Assert
            _repoMock.Verify(r => r.CreateCommissionAsync(
                It.Is<Commission>(c => c.CommissionRate == 7.5m && c.TotalSales == 2000m && c.CommissionValue == 150m)
            ), Times.Once);
        }

        [Fact]
        public async Task CreateCommissionAsync_DadosValidos_ChamaRepositorioUmaVez()
        {
            // Arrange
            var employee = TestData.CreateEmployee();
            employee.CommissionRate = 5m;
            var commission = TestData.CreateCommission(employeeId: employee.EmployeeId);
            commission.StartDate = new DateTime(2025, 3, 1);
            commission.EndDate = new DateTime(2025, 3, 31);

            _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId))
                .ReturnsAsync(employee);
            _repoMock.Setup(r => r.GetOverlappingCommissionAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync((Commission?)null);
            _saleRepoMock.Setup(r => r.GetTotalSalesByEmployeeAndPeriodAsync(employee.EmployeeId, commission.StartDate, commission.EndDate))
                .ReturnsAsync(500m);
            _repoMock.Setup(r => r.CreateCommissionAsync(It.IsAny<Commission>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateCommissionAsync(commission);

            // Assert
            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateCommissionAsync(It.IsAny<Commission>()), Times.Once);
        }

        [Fact]
        public async Task CreateCommissionAsync_ReferenceVazia_LancaArgumentException()
        {
            // Arrange
            var commission = TestData.CreateCommission();
            commission.StartDate = new DateTime(2025, 1, 1);
            commission.EndDate = new DateTime(2025, 1, 31);
            commission.Reference = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateCommissionAsync(commission));
        }

        #endregion

        #region DeleteCommissionAsync

        [Fact]
        public async Task DeleteCommissionAsync_IdValido_ChamaRepositorioUmaVez()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.DeleteCommissionAsync(id)).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteCommissionAsync(id);

            // Assert
            result.Should().Be(1);
            _repoMock.Verify(r => r.DeleteCommissionAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteCommissionAsync_IdVazio_LancaArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteCommissionAsync(Guid.Empty));
        }

        #endregion
    }
}
