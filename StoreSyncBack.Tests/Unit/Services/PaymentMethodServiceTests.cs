using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class PaymentMethodServiceTests
    {
        private readonly Mock<IPaymentMethodRepository> _repoMock;
        private readonly PaymentMethodService _service;

        public PaymentMethodServiceTests()
        {
            _repoMock = new Mock<IPaymentMethodRepository>();
            _service = new PaymentMethodService(_repoMock.Object);
        }

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_DadosValidos_CriaMétodo()
        {
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Cash);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<PaymentMethod>())).ReturnsAsync(pm.PaymentMethodId);

            var result = await _service.CreateAsync(pm);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateAsync(pm), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_NomeVazio_LancaArgumentException()
        {
            var pm = TestData.CreatePaymentMethod();
            pm.Name = "";

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(pm));
        }

        [Fact]
        public async Task CreateAsync_TipoInvalido_LancaArgumentException()
        {
            var pm = TestData.CreatePaymentMethod();
            pm.Type = 99;

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(pm));
        }

        [Fact]
        public async Task CreateAsync_TipoDinheiroComTaxa_LancaInvalidOperationException()
        {
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Cash);
            pm.Rates = new List<PaymentMethodRate> { TestData.CreatePaymentMethodRate() };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(pm));
        }

        [Fact]
        public async Task CreateAsync_TipoPixComTaxa_LancaInvalidOperationException()
        {
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Pix);
            pm.Rates = new List<PaymentMethodRate> { TestData.CreatePaymentMethodRate() };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(pm));
        }

        [Fact]
        public async Task CreateAsync_TipoCartaoSemTaxa_Permitido()
        {
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.CreditCard);
            pm.Rates = null;
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<PaymentMethod>())).ReturnsAsync(pm.PaymentMethodId);

            var result = await _service.CreateAsync(pm);

            result.Should().Be(1);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_MetodoNaoEncontrado_LancaArgumentException()
        {
            var pm = TestData.CreatePaymentMethod();
            _repoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync((PaymentMethod?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(pm));
        }

        [Fact]
        public async Task UpdateAsync_AlterarTipoParaPixComTaxasExistentes_LancaInvalidOperationException()
        {
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Pix);
            var existing = TestData.CreatePaymentMethod(type: PaymentMethodType.CreditCard);
            existing.PaymentMethodId = pm.PaymentMethodId;
            existing.Rates = new List<PaymentMethodRate> { TestData.CreatePaymentMethodRate() };

            _repoMock.Setup(r => r.GetByIdAsync(pm.PaymentMethodId)).ReturnsAsync(existing);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(pm));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_MetodoUsadoEmVenda_LancaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.IsUsedInSalesAsync(id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(id));
        }

        [Fact]
        public async Task DeleteAsync_MetodoNaoUsado_ChamaRepositorioDelete()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.IsUsedInSalesAsync(id)).ReturnsAsync(false);
            _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(1);

            var result = await _service.DeleteAsync(id);

            result.Should().Be(1);
            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        #endregion

        #region AddRateAsync

        [Fact]
        public async Task AddRateAsync_TipoDinheiro_LancaInvalidOperationException()
        {
            var methodId = Guid.NewGuid();
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Cash);
            pm.PaymentMethodId = methodId;
            pm.Rates = new List<PaymentMethodRate>();
            _repoMock.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(pm);

            var rate = TestData.CreatePaymentMethodRate();

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddRateAsync(methodId, rate));
        }

        [Fact]
        public async Task AddRateAsync_TipoPix_LancaInvalidOperationException()
        {
            var methodId = Guid.NewGuid();
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.Pix);
            pm.PaymentMethodId = methodId;
            pm.Rates = new List<PaymentMethodRate>();
            _repoMock.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(pm);

            var rate = TestData.CreatePaymentMethodRate();

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddRateAsync(methodId, rate));
        }

        [Fact]
        public async Task AddRateAsync_ParcelaDuplicada_LancaInvalidOperationException()
        {
            var methodId = Guid.NewGuid();
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.CreditCard);
            pm.PaymentMethodId = methodId;
            pm.Rates = new List<PaymentMethodRate> { TestData.CreatePaymentMethodRate(installments: 2) };
            _repoMock.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(pm);

            var rate = TestData.CreatePaymentMethodRate(installments: 2);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddRateAsync(methodId, rate));
        }

        [Fact]
        public async Task AddRateAsync_DadosValidos_CriaTaxa()
        {
            var methodId = Guid.NewGuid();
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.CreditCard);
            pm.PaymentMethodId = methodId;
            pm.Rates = new List<PaymentMethodRate>();
            _repoMock.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(pm);
            _repoMock.Setup(r => r.AddRateAsync(It.IsAny<PaymentMethodRate>())).ReturnsAsync(Guid.NewGuid());

            var rate = TestData.CreatePaymentMethodRate(installments: 3, ratePercentage: 2.5m);

            var result = await _service.AddRateAsync(methodId, rate);

            result.Should().Be(1);
            _repoMock.Verify(r => r.AddRateAsync(rate), Times.Once);
        }

        [Fact]
        public async Task AddRateAsync_ParcelaMenorQueUm_LancaArgumentException()
        {
            var methodId = Guid.NewGuid();
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.CreditCard);
            pm.PaymentMethodId = methodId;
            pm.Rates = new List<PaymentMethodRate>();
            _repoMock.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(pm);

            var rate = TestData.CreatePaymentMethodRate(installments: 0);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRateAsync(methodId, rate));
        }

        [Fact]
        public async Task AddRateAsync_TaxaNegativa_LancaArgumentException()
        {
            var methodId = Guid.NewGuid();
            var pm = TestData.CreatePaymentMethod(type: PaymentMethodType.DebitCard);
            pm.PaymentMethodId = methodId;
            pm.Rates = new List<PaymentMethodRate>();
            _repoMock.Setup(r => r.GetByIdAsync(methodId)).ReturnsAsync(pm);

            var rate = TestData.CreatePaymentMethodRate(ratePercentage: -1m);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRateAsync(methodId, rate));
        }

        #endregion
    }
}
