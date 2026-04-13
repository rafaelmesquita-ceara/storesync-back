using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _service = new ProductService(_repoMock.Object);
        }

        #region CreateProductAsync - CostPrice

        [Fact]
        public async Task CreateProductAsync_CostPriceNegativo_LancaArgumentException()
        {
            var product = TestData.CreateProduct(price: 100m, stock: 5, costPrice: -1m);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProductAsync(product));

            ex.Message.Should().Contain("CostPrice");
        }

        [Fact]
        public async Task CreateProductAsync_CostPriceZero_CriaComSucesso()
        {
            var product = TestData.CreateProduct(price: 100m, stock: 5, costPrice: 0m);
            _repoMock.Setup(r => r.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(1);

            var result = await _service.CreateProductAsync(product);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateProductAsync(It.Is<Product>(p => p.CostPrice == 0m)), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_CostPricePositivo_CriaComSucesso()
        {
            var product = TestData.CreateProduct(price: 100m, stock: 5, costPrice: 60m);
            _repoMock.Setup(r => r.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(1);

            var result = await _service.CreateProductAsync(product);

            result.Should().Be(1);
            _repoMock.Verify(r => r.CreateProductAsync(It.Is<Product>(p => p.CostPrice == 60m)), Times.Once);
        }

        #endregion

        #region UpdateProductAsync - CostPrice

        [Fact]
        public async Task UpdateProductAsync_CostPriceNegativo_LancaArgumentException()
        {
            var product = TestData.CreateProduct(price: 100m, stock: 5, costPrice: -5m);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateProductAsync(product));

            ex.Message.Should().Contain("CostPrice");
        }

        [Fact]
        public async Task UpdateProductAsync_CostPriceValido_AtualizaComSucesso()
        {
            var product = TestData.CreateProduct(price: 100m, stock: 5, costPrice: 45m);
            _repoMock.Setup(r => r.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(1);

            var result = await _service.UpdateProductAsync(product);

            result.Should().Be(1);
            _repoMock.Verify(r => r.UpdateProductAsync(It.Is<Product>(p => p.CostPrice == 45m)), Times.Once);
        }

        #endregion
    }
}
