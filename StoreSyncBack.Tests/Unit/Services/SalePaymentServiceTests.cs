using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class SalePaymentServiceTests
    {
        private readonly Mock<ISalePaymentRepository> _repoMock;
        private readonly Mock<ISaleRepository> _saleRepoMock;
        private readonly Mock<IPaymentMethodRepository> _pmRepoMock;
        private readonly SalePaymentService _service;

        public SalePaymentServiceTests()
        {
            _repoMock = new Mock<ISalePaymentRepository>();
            _saleRepoMock = new Mock<ISaleRepository>();
            _pmRepoMock = new Mock<IPaymentMethodRepository>();
            _service = new SalePaymentService(_repoMock.Object, _saleRepoMock.Object, _pmRepoMock.Object);
        }

        #region GetAllSalePaymentsAsync

        [Fact]
        public async Task GetAllSalePaymentsAsync_Valido_RetornaPaginado()
        {
            var payments = new List<SalePayment>
            {
                TestData.CreateSalePayment(amount: 100),
                TestData.CreateSalePayment(amount: 200)
            };
            var expected = new PaginatedResult<SalePayment>
            {
                Items = payments,
                TotalCount = 2,
                Limit = 50,
                Offset = 0
            };

            _repoMock.Setup(r => r.GetAllSalePaymentsAsync(50, 0)).ReturnsAsync(expected);

            var result = await _service.GetAllSalePaymentsAsync(50, 0);

            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            _repoMock.Verify(r => r.GetAllSalePaymentsAsync(50, 0), Times.Once);
        }

        #endregion

        #region AddPaymentAsync

        [Fact]
        public async Task AddPaymentAsync_ValorZero_LancaArgumentException()
        {
            var payment = TestData.CreateSalePayment(amount: 0);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddPaymentAsync(payment));
        }

        [Fact]
        public async Task AddPaymentAsync_ValorNegativo_LancaArgumentException()
        {
            var payment = TestData.CreateSalePayment(amount: -50);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddPaymentAsync(payment));
        }

        [Fact]
        public async Task AddPaymentAsync_VendaNaoEncontrada_LancaArgumentException()
        {
            var payment = TestData.CreateSalePayment();
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(payment.SaleId)).ReturnsAsync((Sale?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddPaymentAsync(payment));
        }

        [Fact]
        public async Task AddPaymentAsync_VendaFinalizada_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Finalizada);
            var payment = TestData.CreateSalePayment();
            payment.SaleId = sale.SaleId;
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddPaymentAsync(payment));
        }

        [Fact]
        public async Task AddPaymentAsync_VendaCancelada_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Cancelada);
            var payment = TestData.CreateSalePayment();
            payment.SaleId = sale.SaleId;
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddPaymentAsync(payment));
        }

        [Fact]
        public async Task AddPaymentAsync_MetodoNaoEncontrado_LancaArgumentException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var payment = TestData.CreateSalePayment();
            payment.SaleId = sale.SaleId;
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _pmRepoMock.Setup(r => r.GetByIdAsync(payment.PaymentMethodId)).ReturnsAsync((PaymentMethod?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddPaymentAsync(payment));
        }

        [Fact]
        public async Task AddPaymentAsync_TipoDinheiro_IgnoraParcelasETaxa()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Cash);
            var payment = TestData.CreateSalePayment(amount: 100);
            payment.SaleId = sale.SaleId;
            payment.PaymentMethodId = pm.PaymentMethodId;
            payment.Installments = 3;
            payment.SurchargeApplied = true;
            payment.SurchargeAmount = 10;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _pmRepoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync(pm);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<SalePayment>())).ReturnsAsync(Guid.NewGuid());

            await _service.AddPaymentAsync(payment);

            _repoMock.Verify(r => r.CreateAsync(It.Is<SalePayment>(p =>
                p.Installments == 1 && !p.SurchargeApplied && p.SurchargeAmount == 0)), Times.Once);
        }

        [Fact]
        public async Task AddPaymentAsync_TipoPix_IgnoraParcelasETaxa()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Pix);
            var payment = TestData.CreateSalePayment(amount: 50);
            payment.SaleId = sale.SaleId;
            payment.PaymentMethodId = pm.PaymentMethodId;
            payment.Installments = 2;
            payment.SurchargeApplied = true;
            payment.SurchargeAmount = 5;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _pmRepoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync(pm);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<SalePayment>())).ReturnsAsync(Guid.NewGuid());

            await _service.AddPaymentAsync(payment);

            _repoMock.Verify(r => r.CreateAsync(It.Is<SalePayment>(p =>
                p.Installments == 1 && !p.SurchargeApplied && p.SurchargeAmount == 0)), Times.Once);
        }

        [Fact]
        public async Task AddPaymentAsync_CartaoComTaxaRepassada_IncrementaAdditionDaVenda()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            sale.Addition = 0;
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.CreditCard);
            var payment = TestData.CreateSalePayment(amount: 100, surchargeApplied: true);
            payment.SurchargeAmount = 5;
            payment.SaleId = sale.SaleId;
            payment.PaymentMethodId = pm.PaymentMethodId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _pmRepoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync(pm);
            _saleRepoMock.Setup(r => r.UpdateSaleAsync(It.IsAny<Sale>())).ReturnsAsync(1);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<SalePayment>())).ReturnsAsync(Guid.NewGuid());

            await _service.AddPaymentAsync(payment);

            _saleRepoMock.Verify(r => r.UpdateSaleAsync(It.Is<Sale>(s => s.Addition == 5)), Times.Once);
        }

        [Fact]
        public async Task AddPaymentAsync_CartaoSemRepasse_NaoAlteraAddition()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            sale.Addition = 0;
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.DebitCard);
            var payment = TestData.CreateSalePayment(amount: 100, surchargeApplied: false);
            payment.SurchargeAmount = 0;
            payment.SaleId = sale.SaleId;
            payment.PaymentMethodId = pm.PaymentMethodId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _pmRepoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync(pm);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<SalePayment>())).ReturnsAsync(Guid.NewGuid());

            await _service.AddPaymentAsync(payment);

            _saleRepoMock.Verify(r => r.UpdateSaleAsync(It.IsAny<Sale>()), Times.Never);
        }

        [Fact]
        public async Task AddPaymentAsync_DadosValidos_CriaRegistroNoBanco()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Cash);
            var payment = TestData.CreateSalePayment(amount: 150);
            payment.SaleId = sale.SaleId;
            payment.PaymentMethodId = pm.PaymentMethodId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _pmRepoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync(pm);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<SalePayment>())).ReturnsAsync(Guid.NewGuid());

            var result = await _service.AddPaymentAsync(payment);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<SalePayment>()), Times.Once);
        }

        #endregion

        #region RemovePaymentAsync

        [Fact]
        public async Task RemovePaymentAsync_PagamentoNaoEncontrado_LancaArgumentException()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((SalePayment?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.RemovePaymentAsync(id));
        }

        [Fact]
        public async Task RemovePaymentAsync_VendaFinalizada_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Finalizada);
            var payment = TestData.CreateSalePayment();
            payment.SaleId = sale.SaleId;

            _repoMock.Setup(r => r.GetByIdAsync(payment.SalePaymentId)).ReturnsAsync(payment);
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RemovePaymentAsync(payment.SalePaymentId));
        }

        [Fact]
        public async Task RemovePaymentAsync_ComTaxaRepassada_SubtraiSurchargeDoAddition()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            sale.Addition = 10;
            var payment = TestData.CreateSalePayment(surchargeApplied: true);
            payment.SurchargeAmount = 5;
            payment.SaleId = sale.SaleId;

            _repoMock.Setup(r => r.GetByIdAsync(payment.SalePaymentId)).ReturnsAsync(payment);
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _saleRepoMock.Setup(r => r.UpdateSaleAsync(It.IsAny<Sale>())).ReturnsAsync(1);
            _repoMock.Setup(r => r.DeleteAsync(payment.SalePaymentId)).ReturnsAsync(1);

            await _service.RemovePaymentAsync(payment.SalePaymentId);

            _saleRepoMock.Verify(r => r.UpdateSaleAsync(It.Is<Sale>(s => s.Addition == 5)), Times.Once);
        }

        [Fact]
        public async Task RemovePaymentAsync_SemTaxaRepassada_NaoAlteraAddition()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            sale.Addition = 0;
            var payment = TestData.CreateSalePayment(surchargeApplied: false);
            payment.SurchargeAmount = 0;
            payment.SaleId = sale.SaleId;

            _repoMock.Setup(r => r.GetByIdAsync(payment.SalePaymentId)).ReturnsAsync(payment);
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _repoMock.Setup(r => r.DeleteAsync(payment.SalePaymentId)).ReturnsAsync(1);

            await _service.RemovePaymentAsync(payment.SalePaymentId);

            _saleRepoMock.Verify(r => r.UpdateSaleAsync(It.IsAny<Sale>()), Times.Never);
        }

        #endregion
    }
}
