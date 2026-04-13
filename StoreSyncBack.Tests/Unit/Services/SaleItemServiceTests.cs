using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class SaleItemServiceTests
    {
        private readonly Mock<ISaleItemRepository> _repoMock;
        private readonly Mock<ISaleRepository> _saleRepoMock;
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly SaleItemService _service;

        public SaleItemServiceTests()
        {
            _repoMock = new Mock<ISaleItemRepository>();
            _saleRepoMock = new Mock<ISaleRepository>();
            _productRepoMock = new Mock<IProductRepository>();
            _service = new SaleItemService(_repoMock.Object, _saleRepoMock.Object, _productRepoMock.Object);
        }

        #region CreateSaleItemAsync

        [Fact]
        public async Task CreateSaleItemAsync_EstoqueSuficiente_CriaComSucesso()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var product = TestData.CreateProduct(price: 100m, stock: 10);
            var saleItem = TestData.CreateSaleItem(quantity: 5);
            saleItem.SaleId = sale.SaleId;
            saleItem.ProductId = product.ProductId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _productRepoMock.Setup(r => r.GetProductByIdAsync(product.ProductId)).ReturnsAsync(product);
            _repoMock.Setup(r => r.CreateSaleItemAsync(It.IsAny<SaleItem>())).ReturnsAsync(1);

            var result = await _service.CreateSaleItemAsync(saleItem);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateSaleItemAsync(It.Is<SaleItem>(si =>
                si.TotalPrice == 500m)), Times.Once);
        }

        [Fact]
        public async Task CreateSaleItemAsync_EstoqueInsuficiente_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var product = TestData.CreateProduct(price: 100m, stock: 3);
            var saleItem = TestData.CreateSaleItem(quantity: 5);
            saleItem.SaleId = sale.SaleId;
            saleItem.ProductId = product.ProductId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _productRepoMock.Setup(r => r.GetProductByIdAsync(product.ProductId)).ReturnsAsync(product);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateSaleItemAsync(saleItem));
            ex.Message.Should().Contain("Estoque insuficiente");
        }

        [Fact]
        public async Task CreateSaleItemAsync_VendaNaoAberta_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Finalizada);
            var saleItem = TestData.CreateSaleItem(quantity: 1);
            saleItem.SaleId = sale.SaleId;
            saleItem.ProductId = Guid.NewGuid();

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateSaleItemAsync(saleItem));
            ex.Message.Should().Contain("aberto");
        }

        [Fact]
        public async Task CreateSaleItemAsync_SaleItemNull_LancaArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateSaleItemAsync(null!));
        }

        [Fact]
        public async Task CreateSaleItemAsync_SaleIdVazio_LancaArgumentException()
        {
            var saleItem = TestData.CreateSaleItem();
            saleItem.SaleId = Guid.Empty;

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateSaleItemAsync(saleItem));
        }

        [Fact]
        public async Task CreateSaleItemAsync_QuantidadeZero_LancaArgumentException()
        {
            var saleItem = TestData.CreateSaleItem(quantity: 0);
            saleItem.SaleId = Guid.NewGuid();
            saleItem.ProductId = Guid.NewGuid();

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateSaleItemAsync(saleItem));
        }

        #endregion

        #region CostPrice snapshot

        [Fact]
        public async Task CreateSaleItemAsync_ProdutoComCostPrice_SnapshotCopiadoParaItem()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var product = TestData.CreateProduct(price: 100m, stock: 10, costPrice: 40m);
            var saleItem = TestData.CreateSaleItem(quantity: 2);
            saleItem.SaleId = sale.SaleId;
            saleItem.ProductId = product.ProductId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _productRepoMock.Setup(r => r.GetProductByIdAsync(product.ProductId)).ReturnsAsync(product);
            _repoMock.Setup(r => r.CreateSaleItemAsync(It.IsAny<SaleItem>())).ReturnsAsync(1);

            await _service.CreateSaleItemAsync(saleItem);

            _repoMock.Verify(r => r.CreateSaleItemAsync(It.Is<SaleItem>(si =>
                si.CostPrice == 40m)), Times.Once);
        }

        [Fact]
        public async Task CreateSaleItemAsync_ProdutoSemCostPrice_SnapshotEhZero()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var product = TestData.CreateProduct(price: 100m, stock: 10, costPrice: 0m);
            var saleItem = TestData.CreateSaleItem(quantity: 1);
            saleItem.SaleId = sale.SaleId;
            saleItem.ProductId = product.ProductId;

            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _productRepoMock.Setup(r => r.GetProductByIdAsync(product.ProductId)).ReturnsAsync(product);
            _repoMock.Setup(r => r.CreateSaleItemAsync(It.IsAny<SaleItem>())).ReturnsAsync(1);

            await _service.CreateSaleItemAsync(saleItem);

            _repoMock.Verify(r => r.CreateSaleItemAsync(It.Is<SaleItem>(si =>
                si.CostPrice == 0m)), Times.Once);
        }

        #endregion

        #region DeleteSaleItemAsync

        [Fact]
        public async Task DeleteSaleItemAsync_VendaAberta_RemoveComSucesso()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Aberta);
            var saleItem = TestData.CreateSaleItem();
            saleItem.SaleId = sale.SaleId;

            _repoMock.Setup(r => r.GetSaleItemByIdAsync(saleItem.SaleItemId)).ReturnsAsync(saleItem);
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);
            _repoMock.Setup(r => r.DeleteSaleItemAsync(saleItem.SaleItemId)).ReturnsAsync(1);

            var result = await _service.DeleteSaleItemAsync(saleItem.SaleItemId);

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteSaleItemAsync_VendaFinalizada_LancaInvalidOperationException()
        {
            var sale = TestData.CreateSale(status: SaleStatus.Finalizada);
            var saleItem = TestData.CreateSaleItem();
            saleItem.SaleId = sale.SaleId;

            _repoMock.Setup(r => r.GetSaleItemByIdAsync(saleItem.SaleItemId)).ReturnsAsync(saleItem);
            _saleRepoMock.Setup(r => r.GetSaleByIdAsync(sale.SaleId)).ReturnsAsync(sale);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.DeleteSaleItemAsync(saleItem.SaleItemId));
        }

        #endregion
    }
}
