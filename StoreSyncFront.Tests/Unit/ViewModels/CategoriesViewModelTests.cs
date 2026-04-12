using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Tests.Fixtures;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Tests.Unit.ViewModels;

public class CategoriesViewModelTests
{
    private readonly Mock<ICategoryService> _serviceMock;
    private readonly CategoriesViewModel _vm;

    public CategoriesViewModelTests()
    {
        _serviceMock = new Mock<ICategoryService>();
        _vm = new CategoriesViewModel(_serviceMock.Object);
    }

    #region LoadDataAsync

    [Fact]
    public async Task LoadDataAsync_ComCategorias_PopulaCollection()
    {
        // Arrange
        var cats = TestData.CreateCategories(3);
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(cats));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Categories.Should().HaveCount(3);
        _vm.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task LoadDataAsync_SemCategorias_CollectionVaziaETotalPagesUm()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category>()));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Categories.Should().BeEmpty();
        _vm.TotalPages.Should().Be(1);
    }

    #endregion

    #region CommitEdit

    [Fact]
    public async Task CommitEdit_NomeValido_ChamaUpdateERecarrega()
    {
        // Arrange
        var cat = TestData.CreateCategory("Original");
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category> { cat }));
        await _vm.LoadDataAsync();

        var row = _vm.Categories.First();
        row.DraftName = "Atualizada";

        _serviceMock.Setup(s => s.UpdateCategoryAsync(It.IsAny<Category>())).ReturnsAsync(0);

        // Act
        await _vm.CommitEdit(row);

        // Assert
        _serviceMock.Verify(s => s.UpdateCategoryAsync(It.Is<Category>(c => c.Name == "Atualizada")), Times.Once);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_IdValido_ChamaDeleteERecarrega()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteCategoryAsync(id)).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category>()));

        // Act
        await _vm.DeleteCommand.ExecuteAsync(id);

        // Assert
        _serviceMock.Verify(s => s.DeleteCategoryAsync(id), Times.Once);
    }

    #endregion

    #region BeginEdit / CancelEdit

    [Fact]
    public async Task BeginEdit_CategoriaExistente_MarcaIsEditingVerdadeiro()
    {
        // Arrange
        var cat = TestData.CreateCategory("Eletrônicos");
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category> { cat }));
        await _vm.LoadDataAsync();
        var row = _vm.Categories.First();

        // Act
        _vm.BeginEdit(cat.CategoryId);

        // Assert
        row.IsEditing.Should().BeTrue();
        row.DraftName.Should().Be("Eletrônicos");
    }

    [Fact]
    public async Task CancelEdit_Sempre_LimpaIsEditingERestauraDraftName()
    {
        // Arrange
        var cat = TestData.CreateCategory("Roupas");
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category> { cat }));
        await _vm.LoadDataAsync();
        var row = _vm.Categories.First();
        row.DraftName = "Alterado";

        // Act
        _vm.CancelEdit(row);

        // Assert
        row.IsEditing.Should().BeFalse();
        row.DraftName.Should().Be("Roupas");
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_TermoEncontrado_FiltraCollection()
    {
        // Arrange
        var c1 = TestData.CreateCategory("Eletrônicos");
        var c2 = TestData.CreateCategory("Roupas");
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category> { c1, c2 }));
        await _vm.LoadDataAsync();

        // Act
        _vm.SearchBarField = "Eletronicos";
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Categories.Should().HaveCount(1);
        _vm.Categories[0].Name.Should().Be("Eletrônicos");
    }

    [Fact]
    public async Task Search_TermoVazio_RestauraTodas()
    {
        // Arrange
        var cats = TestData.CreateCategories(4);
        _serviceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(cats));
        await _vm.LoadDataAsync();
        _vm.SearchBarField = "xyz";
        _vm.SearchCommand.Execute(null);

        // Act
        _vm.SearchBarField = string.Empty;
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Categories.Should().HaveCount(4);
    }

    #endregion

    #region Paginação

    [Fact]
    public void CanPreviousPage_PrimeiraPagina_Falso()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages = 2;
        _vm.CanPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void CanNextPage_TemProximaPagina_Verdadeiro()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages = 2;
        _vm.CanNextPage.Should().BeTrue();
    }

    #endregion
}
