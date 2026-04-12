using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class CaixaServiceTests
    {
        private readonly Mock<ICaixaRepository> _repoMock;
        private readonly Mock<CaixaPdfReportService> _pdfMock;
        private readonly CaixaService _service;

        public CaixaServiceTests()
        {
            _repoMock = new Mock<ICaixaRepository>();
            _pdfMock = new Mock<CaixaPdfReportService>();
            _service = new CaixaService(_repoMock.Object, _pdfMock.Object);
        }

        #region AbrirCaixaAsync

        [Fact]
        public async Task AbrirCaixaAsync_QuandoNenhumCaixaAberto_CriaCaixaComSucesso()
        {
            _repoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync((Caixa?)null);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Caixa>())).ReturnsAsync(Guid.NewGuid());

            var result = await _service.AbrirCaixaAsync(100m);

            result.Should().NotBeNull();
            result.ValorAbertura.Should().Be(100m);
            result.Status.Should().Be(CaixaStatus.Aberto);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Caixa>()), Times.Once);
        }

        [Fact]
        public async Task AbrirCaixaAsync_QuandoCaixaJaAberto_LancaInvalidOperationException()
        {
            var caixaAberto = TestData.CreateCaixa(status: CaixaStatus.Aberto);
            _repoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync(caixaAberto);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AbrirCaixaAsync(100m));
        }

        #endregion

        #region FecharCaixaAsync

        [Fact]
        public async Task FecharCaixaAsync_QuandoCaixaNaoEncontrado_LancaArgumentException()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Caixa?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.FecharCaixaAsync(Guid.NewGuid(), 100m));
        }

        [Fact]
        public async Task FecharCaixaAsync_QuandoCaixaFechado_LancaInvalidOperationException()
        {
            var caixa = TestData.CreateCaixa(status: CaixaStatus.Fechado);
            _repoMock.Setup(r => r.GetByIdAsync(caixa.CaixaId)).ReturnsAsync(caixa);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.FecharCaixaAsync(caixa.CaixaId, 100m));
        }

        [Fact]
        public async Task FecharCaixaAsync_QuandoValorFechamentoMenorQueEsperado_CalculaFaltante()
        {
            var caixa = TestData.CreateCaixa(status: CaixaStatus.Aberto, valorAbertura: 100m);
            var vendas = new List<Sale> { TestData.CreateSale(totalAmount: 50m, status: SaleStatus.Finalizada) };

            _repoMock.Setup(r => r.GetByIdAsync(caixa.CaixaId)).ReturnsAsync(caixa);
            _repoMock.Setup(r => r.GetVendasByCaixaAsync(caixa.CaixaId)).ReturnsAsync(vendas);
            _repoMock.Setup(r => r.GetMovimentacoesByCaixaAsync(caixa.CaixaId)).ReturnsAsync(new List<MovimentacaoCaixa>());
            _repoMock.Setup(r => r.FecharAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal?>(), It.IsAny<decimal?>())).ReturnsAsync(1);

            // saldoEsperado = 100 + 50 = 150; valorFechamento = 145; diferenca = -5 → faltante
            await _service.FecharCaixaAsync(caixa.CaixaId, 145m);

            _repoMock.Verify(r => r.FecharAsync(
                caixa.CaixaId,
                145m,
                50m,
                0m,
                0m,
                5m,   // valorFaltante
                null  // valorSobra
            ), Times.Once);
        }

        [Fact]
        public async Task FecharCaixaAsync_QuandoValorFechamentoMaiorQueEsperado_CalculaSobra()
        {
            var caixa = TestData.CreateCaixa(status: CaixaStatus.Aberto, valorAbertura: 100m);
            var vendas = new List<Sale> { TestData.CreateSale(totalAmount: 50m, status: SaleStatus.Finalizada) };

            _repoMock.Setup(r => r.GetByIdAsync(caixa.CaixaId)).ReturnsAsync(caixa);
            _repoMock.Setup(r => r.GetVendasByCaixaAsync(caixa.CaixaId)).ReturnsAsync(vendas);
            _repoMock.Setup(r => r.GetMovimentacoesByCaixaAsync(caixa.CaixaId)).ReturnsAsync(new List<MovimentacaoCaixa>());
            _repoMock.Setup(r => r.FecharAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal?>(), It.IsAny<decimal?>())).ReturnsAsync(1);

            // saldoEsperado = 100 + 50 = 150; valorFechamento = 160; diferenca = +10 → sobra
            await _service.FecharCaixaAsync(caixa.CaixaId, 160m);

            _repoMock.Verify(r => r.FecharAsync(
                caixa.CaixaId,
                160m,
                50m,
                0m,
                0m,
                null, // valorFaltante
                10m   // valorSobra
            ), Times.Once);
        }

        [Fact]
        public async Task FecharCaixaAsync_ComSangriaESuprimento_CalculaCorretamente()
        {
            var caixa = TestData.CreateCaixa(status: CaixaStatus.Aberto, valorAbertura: 200m);
            var vendas = new List<Sale> { TestData.CreateSale(totalAmount: 100m, status: SaleStatus.Finalizada) };
            var movimentacoes = new List<MovimentacaoCaixa>
            {
                TestData.CreateMovimentacao(tipo: MovimentacaoTipo.Sangria, valor: 30m),
                TestData.CreateMovimentacao(tipo: MovimentacaoTipo.Suprimento, valor: 20m)
            };

            _repoMock.Setup(r => r.GetByIdAsync(caixa.CaixaId)).ReturnsAsync(caixa);
            _repoMock.Setup(r => r.GetVendasByCaixaAsync(caixa.CaixaId)).ReturnsAsync(vendas);
            _repoMock.Setup(r => r.GetMovimentacoesByCaixaAsync(caixa.CaixaId)).ReturnsAsync(movimentacoes);
            _repoMock.Setup(r => r.FecharAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal?>(), It.IsAny<decimal?>())).ReturnsAsync(1);

            // saldoEsperado = 200 + 100 + 20 - 30 = 290; valorFechamento = 290 → sem faltante/sobra
            await _service.FecharCaixaAsync(caixa.CaixaId, 290m);

            _repoMock.Verify(r => r.FecharAsync(
                caixa.CaixaId,
                290m,
                100m,
                30m,
                20m,
                null, // valorFaltante
                null  // valorSobra
            ), Times.Once);
        }

        #endregion

        #region AddMovimentacaoAsync

        [Fact]
        public async Task AddMovimentacaoAsync_QuandoCaixaFechado_LancaInvalidOperationException()
        {
            _repoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync((Caixa?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.AddMovimentacaoAsync(Guid.NewGuid(), MovimentacaoTipo.Sangria, "Teste", 50m));
        }

        [Fact]
        public async Task AddMovimentacaoAsync_QuandoValorZero_LancaArgumentException()
        {
            var caixa = TestData.CreateCaixa(status: CaixaStatus.Aberto);
            _repoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync(caixa);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddMovimentacaoAsync(caixa.CaixaId, MovimentacaoTipo.Sangria, "Teste", 0m));
        }

        [Fact]
        public async Task AddMovimentacaoAsync_QuandoDadosValidos_AdicionaMovimentacao()
        {
            var caixa = TestData.CreateCaixa(status: CaixaStatus.Aberto);
            _repoMock.Setup(r => r.GetCaixaAbertoAsync()).ReturnsAsync(caixa);
            _repoMock.Setup(r => r.AddMovimentacaoAsync(It.IsAny<MovimentacaoCaixa>())).ReturnsAsync(1);

            await _service.AddMovimentacaoAsync(caixa.CaixaId, MovimentacaoTipo.Sangria, "Retirada", 50m);

            _repoMock.Verify(r => r.AddMovimentacaoAsync(It.Is<MovimentacaoCaixa>(m =>
                m.CaixaId == caixa.CaixaId &&
                m.Tipo == MovimentacaoTipo.Sangria &&
                m.Valor == 50m)), Times.Once);
        }

        #endregion
    }
}
