using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class SaleServiceTests
    {
        private readonly Mock<ISaleRepository> _repoMock;
        private readonly Mock<ISalePaymentRepository> _salePaymentRepoMock;
        private readonly Mock<ICaixaRepository> _caixaRepoMock;
        private readonly SaleService _service;

        public SaleServiceTests()
        {
            _repoMock = new Mock<ISaleRepository>();
            _salePaymentRepoMock = new Mock<ISalePaymentRepository>();
            _caixaRepoMock = new Mock<ICaixaRepository>();
            // Por padrão, simula um caixa aberto para não quebrar testes existentes
            _caixaRepoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync(TestData.CreateCaixa());
            _service = new SaleService(_repoMock.Object, _salePaymentRepoMock.Object, _caixaRepoMock.Object);
        }

        #region CreateSaleAsync

        [Fact]
        public async Task CreateSaleAsync_DadosValidos_CriaVendaAberta()
        {
            var sale = TestData.CreateSale();
            _repoMock.Setup(r => r.CreateSaleAsync(It.IsAny<Sale>())).ReturnsAsync(Guid.NewGuid());

            var result = await _service.CreateSaleAsync(sale);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateSaleAsync(It.Is<Sale>(s =>
                s.Status == SaleStatus.Aberta && s.TotalAmount == 0)), Times.Once);
        }

        [Fact]
        public async Task CreateSaleAsync_SaleNull_LancaArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateSaleAsync(null!));
        }

        [Fact]
        public async Task CreateSaleAsync_EmployeeIdVazio_LancaArgumentException()
        {
            var sale = TestData.CreateSale();
            sale.EmployeeId = Guid.Empty;

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateSaleAsync(sale));
        }

        [Fact]
        public async Task CreateSaleAsync_SemCaixaAberto_LancaInvalidOperationException()
        {
            _caixaRepoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync((Caixa?)null);
            var sale = TestData.CreateSale();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateSaleAsync(sale));
        }

        #endregion

        #region UpdateSaleAsync

        [Fact]
        public async Task UpdateSaleAsync_VendaAberta_AtualizaComSucesso()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            _repoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _repoMock.Setup(r => r.UpdateSaleAsync(It.IsAny<Sale>())).ReturnsAsync(1);

            var result = await _service.UpdateSaleAsync(sale);

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateSaleAsync_VendaFinalizada_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Finalizada);
            _repoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateSaleAsync(sale));
        }

        [Fact]
        public async Task UpdateSaleAsync_SaleIdVazio_LancaArgumentException()
        {
            var sale = TestData.CreateSale();
            sale.SaleId = Guid.Empty;

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateSaleAsync(sale));
        }

        #endregion

        #region FinalizeSaleAsync

        [Fact]
        public async Task FinalizeSaleAsync_SaleIdVazio_LancaArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.FinalizeSaleAsync(Guid.Empty));
        }

        [Fact]
        public async Task FinalizeSaleAsync_PagamentoInsuficiente_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(totalAmount: 100, status: SaleStatus.Aberta);
            sale.Items = TestData.CreateSaleItems(1);
            _repoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _salePaymentRepoMock.Setup(r => r.GetTotalPaidBySaleIdAsync(sale.SaleId)).ReturnsAsync(80m);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.FinalizeSaleAsync(sale.SaleId));

            ex.Message.Should().Contain("Pagamento insuficiente");
        }

        [Fact]
        public async Task FinalizeSaleAsync_SemPagamentos_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(totalAmount: 100, status: SaleStatus.Aberta);
            sale.Items = TestData.CreateSaleItems(1);
            _repoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _salePaymentRepoMock.Setup(r => r.GetTotalPaidBySaleIdAsync(sale.SaleId)).ReturnsAsync(0m);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.FinalizeSaleAsync(sale.SaleId));
        }

        [Fact]
        public async Task FinalizeSaleAsync_PagamentoExato_TrocoZero()
        {
            var sale = TestData.CreateSale(totalAmount: 100, status: SaleStatus.Aberta);
            sale.Items = TestData.CreateSaleItems(1);
            _repoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _salePaymentRepoMock.Setup(r => r.GetTotalPaidBySaleIdAsync(sale.SaleId)).ReturnsAsync(100m);
            _repoMock.Setup(r => r.FinalizeSaleAsync(sale.SaleId, 0m)).ReturnsAsync(1);

            var result = await _service.FinalizeSaleAsync(sale.SaleId);

            result.Should().Be(1);
            _repoMock.Verify(r => r.FinalizeSaleAsync(sale.SaleId, 0m), Times.Once);
        }

        [Fact]
        public async Task FinalizeSaleAsync_PagamentoExcedente_CalculaTroco()
        {
            var sale = TestData.CreateSale(totalAmount: 100, status: SaleStatus.Aberta);
            sale.Items = TestData.CreateSaleItems(1);
            _repoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _salePaymentRepoMock.Setup(r => r.GetTotalPaidBySaleIdAsync(sale.SaleId)).ReturnsAsync(150m);
            _repoMock.Setup(r => r.FinalizeSaleAsync(sale.SaleId, 50m)).ReturnsAsync(1);

            var result = await _service.FinalizeSaleAsync(sale.SaleId);

            result.Should().Be(1);
            _repoMock.Verify(r => r.FinalizeSaleAsync(sale.SaleId, 50m), Times.Once);
        }

        #endregion

        #region CancelSaleAsync

        [Fact]
        public async Task CancelSaleAsync_SaleIdValido_ChamaRepositorio()
        {
            var saleId = Guid.NewGuid();
            _repoMock.Setup(r => r.CancelSaleAsync(saleId)).ReturnsAsync(1);

            var result = await _service.CancelSaleAsync(saleId);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CancelSaleAsync(saleId), Times.Once);
        }

        [Fact]
        public async Task CancelSaleAsync_SaleIdVazio_LancaArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CancelSaleAsync(Guid.Empty));
        }

        #endregion
    }
}
