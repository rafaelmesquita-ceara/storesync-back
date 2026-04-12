using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Tests.Fixtures;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Tests.Unit.ViewModels;

public class ProductsViewModelTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly ProductsViewModel _vm;

    public ProductsViewModelTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _categoryServiceMock = new Mock<ICategoryService>();

        // Stub vazio para categorias (chamado em LoadDataAsync)
        _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Category>()));

        _vm = new ProductsViewModel(_productServiceMock.Object, _categoryServiceMock.Object);
    }

    #region LoadDataAsync

    [Fact]
    public async Task LoadDataAsync_ComProdutos_PopulaCollection()
    {
        // Arrange
        var products = TestData.CreateProducts(3);
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(products));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Products.Should().HaveCount(3);
        _vm.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task LoadDataAsync_SemProdutos_CollectionVaziaETotalPagesUm()
    {
        // Arrange
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Product>()));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Products.Should().BeEmpty();
        _vm.TotalPages.Should().Be(1);
    }

    #endregion

    #region AddProduct — criação

    [Fact]
    public async Task AddProduct_DadosValidos_ChamaCreateERecarrega()
    {
        // Arrange
        _vm.Name = "Produto Teste";
        _vm.Reference = "REF001";
        _vm.Price = "99,90";
        _vm.StockQuantity = "10";
        _vm.SelectedCategory = TestData.CreateCategory();

        _productServiceMock.Setup(s => s.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(1);
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Product>()));

        // Act
        await _vm.AddProductCommand.ExecuteAsync(null);

        // Assert
        _productServiceMock.Verify(s => s.CreateProductAsync(It.Is<Product>(p => p.Name == "Produto Teste")), Times.Once);
    }

    [Fact]
    public async Task AddProduct_NomeVazio_NaoChamaCreate()
    {
        // Arrange
        _vm.Name = string.Empty;
        _vm.Reference = "REF001";
        _vm.Price = "10,00";
        _vm.StockQuantity = "5";

        // Act
        await _vm.AddProductCommand.ExecuteAsync(null);

        // Assert
        _productServiceMock.Verify(s => s.CreateProductAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task AddProduct_PrecoInvalido_NaoChamaCreate()
    {
        // Arrange
        _vm.Name = "Produto";
        _vm.Reference = "REF002";
        _vm.Price = "abc";
        _vm.StockQuantity = "5";

        // Act
        await _vm.AddProductCommand.ExecuteAsync(null);

        // Assert
        _productServiceMock.Verify(s => s.CreateProductAsync(It.IsAny<Product>()), Times.Never);
    }

    #endregion

    #region AddProduct — edição

    [Fact]
    public async Task AddProduct_ComProductIdPreenchido_ChamaUpdateEmVezDeCreate()
    {
        // Arrange
        var id = Guid.NewGuid();
        _vm.ProductId = id;
        _vm.Name = "Produto Atualizado";
        _vm.Reference = "REF003";
        _vm.Price = "50,00";
        _vm.StockQuantity = "3";

        _productServiceMock.Setup(s => s.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(1);
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Product>()));

        // Act
        await _vm.AddProductCommand.ExecuteAsync(null);

        // Assert
        _productServiceMock.Verify(s => s.UpdateProductAsync(It.Is<Product>(p => p.ProductId == id)), Times.Once);
        _productServiceMock.Verify(s => s.CreateProductAsync(It.IsAny<Product>()), Times.Never);
    }

    #endregion

    #region OpenEdit

    [Fact]
    public async Task OpenEdit_ProdutoExistente_PopulaFormulario()
    {
        // Arrange
        var product = TestData.CreateProduct(price: 25m, stock: 7);
        product.Name = "Caneta";
        product.Reference = "REF-CAN";
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Product> { product }));
        await _vm.LoadDataAsync();

        // Act
        _vm.OpenEditCommand.Execute(product.ProductId);

        // Assert
        _vm.ProductId.Should().Be(product.ProductId);
        _vm.Name.Should().Be("Caneta");
        _vm.Reference.Should().Be("REF-CAN");
    }

    [Fact]
    public void OpenEdit_ProdutoInexistente_NaoAlteraEstado()
    {
        // Arrange
        var nameBefore = _vm.Name;

        // Act
        _vm.OpenEditCommand.Execute(Guid.NewGuid());

        // Assert
        _vm.Name.Should().Be(nameBefore);
    }

    #endregion

    #region ClearForm

    [Fact]
    public void ClearForm_Sempre_ResetaCampos()
    {
        // Arrange
        _vm.Name = "Algum produto";
        _vm.Reference = "REF999";
        _vm.ProductId = Guid.NewGuid();

        // Act
        _vm.ClearFormCommand.Execute(null);

        // Assert
        _vm.Name.Should().BeEmpty();
        _vm.Reference.Should().BeEmpty();
        _vm.ProductId.Should().Be(Guid.Empty);
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_TermoEncontrado_FiltraPorNome()
    {
        // Arrange
        var p1 = TestData.CreateProduct(); p1.Name = "Caderno";
        var p2 = TestData.CreateProduct(); p2.Name = "Borracha";
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Product> { p1, p2 }));
        await _vm.LoadDataAsync();

        // Act
        _vm.SearchBarField = "Caderno";
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Products.Should().HaveCount(1);
        _vm.Products[0].Name.Should().Be("Caderno");
    }

    [Fact]
    public async Task Search_TermoVazio_RestauraTodos()
    {
        // Arrange
        var products = TestData.CreateProducts(3);
        _productServiceMock.Setup(s => s.GetAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(products));
        await _vm.LoadDataAsync();
        _vm.SearchBarField = "xyz";
        _vm.SearchCommand.Execute(null);

        // Act
        _vm.SearchBarField = string.Empty;
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Products.Should().HaveCount(3);
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
        _vm.TotalPages = 3;
        _vm.CanNextPage.Should().BeTrue();
    }

    #endregion
}
