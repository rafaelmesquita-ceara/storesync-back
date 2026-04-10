using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class FinanceServiceTests
    {
        private readonly Mock<IFinanceRepository> _repoMock;
        private readonly FinanceService _service;

        public FinanceServiceTests()
        {
            _repoMock = new Mock<IFinanceRepository>();
            _service = new FinanceService(_repoMock.Object);
        }

        #region CreateFinanceAsync

        [Fact]
        public async Task CreateFinanceAsync_DadosValidos_RetornaLinhasAfetadas()
        {
            // Arrange
            var finance = TestData.CreateFinance();
            _repoMock.Setup(r => r.CreateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(1);

            // Act
            var result = await _service.CreateFinanceAsync(finance);

            // Assert
            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateFinanceAsync(It.IsAny<Finance>()), Times.Once);
        }

        [Fact]
        public async Task CreateFinanceAsync_FinanceNull_LancaArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateFinanceAsync(null!));
        }

        [Fact]
        public async Task CreateFinanceAsync_DescricaoVazia_LancaArgumentException()
        {
            // Arrange
            var finance = TestData.CreateFinance();
            finance.Description = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateFinanceAsync(finance));
        }

        [Fact]
        public async Task CreateFinanceAsync_ValorZero_LancaArgumentException()
        {
            // Arrange
            var finance = TestData.CreateFinance();
            finance.Amount = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateFinanceAsync(finance));
        }

        [Fact]
        public async Task CreateFinanceAsync_StatusZero_DefinePadraoAberto()
        {
            // Arrange
            var finance = TestData.CreateFinance();
            finance.Status = 0;
            _repoMock.Setup(r => r.CreateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(1);

            // Act
            await _service.CreateFinanceAsync(finance);

            // Assert
            _repoMock.Verify(r => r.CreateFinanceAsync(
                It.Is<Finance>(f => f.Status == FinanceStatus.Aberto)), Times.Once);
        }

        #endregion

        #region DeleteFinanceAsync

        [Fact]
        public async Task DeleteFinanceAsync_StatusAberto_DeletaComSucesso()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.Aberto);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);
            _repoMock.Setup(r => r.DeleteFinanceAsync(finance.FinanceId)).ReturnsAsync(1);

            // Act
            var result = await _service.DeleteFinanceAsync(finance.FinanceId);

            // Assert
            result.Should().Be(1);
            _repoMock.Verify(r => r.DeleteFinanceAsync(finance.FinanceId), Times.Once);
        }

        [Fact]
        public async Task DeleteFinanceAsync_StatusNaoAberto_LancaInvalidOperationException()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.Liquidado);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteFinanceAsync(finance.FinanceId));
            ex.Message.Should().Contain("aberto");
        }

        [Fact]
        public async Task DeleteFinanceAsync_StatusLiquidadoParcialmente_LancaInvalidOperationException()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.LiquidadoParcialmente);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteFinanceAsync(finance.FinanceId));
        }

        [Fact]
        public async Task DeleteFinanceAsync_FinanceNaoEncontrado_LancaKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetFinanceByIdAsync(id)).ReturnsAsync((Finance?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteFinanceAsync(id));
        }

        #endregion

        #region SettleAsync

        [Fact]
        public async Task SettleAsync_ValorIgualAmount_StatusLiquidado()
        {
            // Arrange
            var finance = TestData.CreateFinanceWithAmount(500m);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);
            _repoMock.Setup(r => r.SettleAsync(finance.FinanceId, 500m, It.IsAny<DateTime>(), It.IsAny<string?>(), FinanceStatus.Liquidado))
                     .ReturnsAsync(1);

            // Act
            await _service.SettleAsync(finance.FinanceId, 500m, "Liquidado integralmente");

            // Assert
            _repoMock.Verify(r => r.SettleAsync(
                finance.FinanceId, 500m, It.IsAny<DateTime>(), "Liquidado integralmente", FinanceStatus.Liquidado),
                Times.Once);
        }

        [Fact]
        public async Task SettleAsync_ValorMenorQueAmount_StatusLiquidadoParcialmenteCriaResidual()
        {
            // Arrange
            var finance = TestData.CreateFinanceWithAmount(500m);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);
            _repoMock.Setup(r => r.SettleAsync(finance.FinanceId, 200m, It.IsAny<DateTime>(), It.IsAny<string?>(), FinanceStatus.LiquidadoParcialmente))
                     .ReturnsAsync(1);
            _repoMock.Setup(r => r.CreateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(1);

            // Act
            await _service.SettleAsync(finance.FinanceId, 200m, null);

            // Assert
            _repoMock.Verify(r => r.SettleAsync(
                finance.FinanceId, 200m, It.IsAny<DateTime>(), null, FinanceStatus.LiquidadoParcialmente),
                Times.Once);

            _repoMock.Verify(r => r.CreateFinanceAsync(It.Is<Finance>(f =>
                f.Amount == 300m &&
                f.TitleType == FinanceTitleType.Residual &&
                f.ParentId == finance.FinanceId &&
                f.Status == FinanceStatus.Aberto)),
                Times.Once);
        }

        [Fact]
        public async Task SettleAsync_ValorMaiorQueAmount_LancaInvalidOperationException()
        {
            // Arrange
            var finance = TestData.CreateFinanceWithAmount(500m);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SettleAsync(finance.FinanceId, 600m, null));
            ex.Message.Should().Contain("maior que o valor da conta");
        }

        [Fact]
        public async Task SettleAsync_ValorZero_LancaArgumentException()
        {
            // Arrange
            var finance = TestData.CreateFinanceWithAmount(500m);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.SettleAsync(finance.FinanceId, 0m, null));
        }

        [Fact]
        public async Task SettleAsync_FinanceNaoEncontrado_LancaKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetFinanceByIdAsync(id)).ReturnsAsync((Finance?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.SettleAsync(id, 100m, null));
        }

        #endregion

        #region CancelSettlementAsync

        [Fact]
        public async Task CancelSettlementAsync_StatusLiquidado_ResetaStatusParaAberto()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.Liquidado);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);
            _repoMock.Setup(r => r.CancelSettlementAsync(finance.FinanceId)).ReturnsAsync(1);

            // Act
            await _service.CancelSettlementAsync(finance.FinanceId);

            // Assert
            _repoMock.Verify(r => r.CancelSettlementAsync(finance.FinanceId), Times.Once);
        }

        [Fact]
        public async Task CancelSettlementAsync_StatusLiquidadoParcialmente_ResidualExiste_LancaInvalidOperationException()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.LiquidadoParcialmente);
            var residual = TestData.CreateFinance();
            residual.ParentId = finance.FinanceId;
            residual.TitleType = FinanceTitleType.Residual;

            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);
            _repoMock.Setup(r => r.GetResidualByParentIdAsync(finance.FinanceId)).ReturnsAsync(residual);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CancelSettlementAsync(finance.FinanceId));
            ex.Message.Should().Contain("residual");
        }

        [Fact]
        public async Task CancelSettlementAsync_StatusLiquidadoParcialmente_ResidualNaoExiste_CancelaComSucesso()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.LiquidadoParcialmente);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);
            _repoMock.Setup(r => r.GetResidualByParentIdAsync(finance.FinanceId)).ReturnsAsync((Finance?)null);
            _repoMock.Setup(r => r.CancelSettlementAsync(finance.FinanceId)).ReturnsAsync(1);

            // Act
            await _service.CancelSettlementAsync(finance.FinanceId);

            // Assert
            _repoMock.Verify(r => r.CancelSettlementAsync(finance.FinanceId), Times.Once);
        }

        [Fact]
        public async Task CancelSettlementAsync_StatusAberto_LancaInvalidOperationException()
        {
            // Arrange
            var finance = TestData.CreateFinance(status: FinanceStatus.Aberto);
            _repoMock.Setup(r => r.GetFinanceByIdAsync(finance.FinanceId)).ReturnsAsync(finance);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CancelSettlementAsync(finance.FinanceId));
        }

        [Fact]
        public async Task CancelSettlementAsync_FinanceNaoEncontrado_LancaKeyNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetFinanceByIdAsync(id)).ReturnsAsync((Finance?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CancelSettlementAsync(id));
        }

        #endregion

        #region GetAllByTypeAsync

        [Fact]
        public async Task GetAllByTypeAsync_TipoPagar_RetornaApenasContasAPagar()
        {
            // Arrange
            var finances = TestData.CreateFinances(3);
            foreach (var f in finances) f.Type = FinanceType.Pagar;

            _repoMock.Setup(r => r.GetAllByTypeAsync(FinanceType.Pagar)).ReturnsAsync(finances);

            // Act
            var result = await _service.GetAllByTypeAsync(FinanceType.Pagar);

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(f => f.Type == FinanceType.Pagar);
            _repoMock.Verify(r => r.GetAllByTypeAsync(FinanceType.Pagar), Times.Once);
        }

        #endregion
    }
}
