using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepoMock = new Mock<ICategoryRepository>();
            _categoryService = new CategoryService(_categoryRepoMock.Object);
        }

        #region GetAllCategoriesAsync

        [Fact]
        public async Task GetAllCategoriesAsync_CategoriesExistem_RetornaListaDeCategorias()
        {
            // Arrange
            var expectedCategories = TestData.CreateCategories(3);
            _categoryRepoMock.Setup(r => r.GetAllCategoriesAsync())
                .ReturnsAsync(expectedCategories);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedCategories);
            _categoryRepoMock.Verify(r => r.GetAllCategoriesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_NenhumaCategoria_RetornaListaVazia()
        {
            // Arrange
            _categoryRepoMock.Setup(r => r.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetCategoryByIdAsync

        [Fact]
        public async Task GetCategoryByIdAsync_CategoriaExistente_RetornaCategoria()
        {
            // Arrange
            var expectedCategory = TestData.CreateCategory("Eletrônicos");
            _categoryRepoMock.Setup(r => r.GetCategoryByIdAsync(expectedCategory.CategoryId))
                .ReturnsAsync(expectedCategory);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(expectedCategory.CategoryId);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryId.Should().Be(expectedCategory.CategoryId);
            result.Name.Should().Be("Eletrônicos");
            _categoryRepoMock.Verify(r => r.GetCategoryByIdAsync(expectedCategory.CategoryId), Times.Once);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_CategoriaInexistente_RetornaNull()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _categoryRepoMock.Setup(r => r.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateCategoryAsync

        [Fact]
        public async Task CreateCategoryAsync_CategoriaValida_RetornaIdCriado()
        {
            // Arrange
            var category = TestData.CreateCategory("Livros");
            _categoryRepoMock.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync(1);

            // Act
            var result = await _categoryService.CreateCategoryAsync(category);

            // Assert
            result.Should().Be(1);
            _categoryRepoMock.Verify(r => r.CreateCategoryAsync(It.Is<Category>(
                c => c.Name == "Livros")), Times.Once);
        }

        [Fact]
        public async Task CreateCategoryAsync_CategoriaNull_LancaArgumentNullException()
        {
            // Arrange
            Category? category = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _categoryService.CreateCategoryAsync(category!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateCategoryAsync_NomeInvalido_LancaArgumentException(string? name)
        {
            // Arrange
            var category = TestData.CreateCategory(name ?? "");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.CreateCategoryAsync(category));
        }

        [Fact]
        public async Task CreateCategoryAsync_CreatedAtPadrao_DefineDataAtual()
        {
            // Arrange
            var category = TestData.CreateCategory("Teste");
            category.CreatedAt = default;

            _categoryRepoMock.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync(1);

            // Act
            await _categoryService.CreateCategoryAsync(category);

            // Assert
            _categoryRepoMock.Verify(r => r.CreateCategoryAsync(It.Is<Category>(
                c => c.CreatedAt != default)), Times.Once);
        }

        #endregion

        #region UpdateCategoryAsync

        [Fact]
        public async Task UpdateCategoryAsync_CategoriaValida_RetornaLinhasAfetadas()
        {
            // Arrange
            var category = TestData.CreateCategory("Roupas");
            _categoryRepoMock.Setup(r => r.UpdateCategoryAsync(It.IsAny<Category>()))
                .ReturnsAsync(1);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(category);

            // Assert
            result.Should().Be(1);
            _categoryRepoMock.Verify(r => r.UpdateCategoryAsync(category), Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoriaNull_LancaArgumentNullException()
        {
            // Arrange
            Category? category = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _categoryService.UpdateCategoryAsync(category!));
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryIdVazio_LancaArgumentException()
        {
            // Arrange
            var category = TestData.CreateCategory();
            category.CategoryId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.UpdateCategoryAsync(category));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateCategoryAsync_NomeInvalido_LancaArgumentException(string? name)
        {
            // Arrange
            var category = TestData.CreateCategory(name ?? "");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.UpdateCategoryAsync(category));
        }

        #endregion

        #region DeleteCategoryAsync

        [Fact]
        public async Task DeleteCategoryAsync_IdValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            _categoryRepoMock.Setup(r => r.DeleteCategoryAsync(categoryId))
                .ReturnsAsync(1);

            // Act
            var result = await _categoryService.DeleteCategoryAsync(categoryId);

            // Assert
            result.Should().Be(1);
            _categoryRepoMock.Verify(r => r.DeleteCategoryAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_IdVazio_LancaArgumentException()
        {
            // Arrange
            var categoryId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.DeleteCategoryAsync(categoryId));
        }

        #endregion
    }
}
