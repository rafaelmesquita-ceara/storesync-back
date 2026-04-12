using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Tests.Fixtures;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Tests.Unit.ViewModels;

public class ClientsViewModelTests
{
    private readonly Mock<IClientService> _serviceMock;
    private readonly ClientsViewModel _vm;

    public ClientsViewModelTests()
    {
        _serviceMock = new Mock<IClientService>();
        _vm = new ClientsViewModel(_serviceMock.Object);
    }

    #region LoadDataAsync

    [Fact]
    public async Task LoadDataAsync_ComClientes_PopulaCollectionECalculaPaginas()
    {
        // Arrange
        var clients = TestData.CreateClients(3);
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(clients));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Clients.Should().HaveCount(3);
        _vm.TotalCount.Should().Be(3);
        _vm.TotalPages.Should().Be(1);
        _vm.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task LoadDataAsync_SemClientes_CollectionVaziaETotalPagesUm()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client>()));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Clients.Should().BeEmpty();
        _vm.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task LoadDataAsync_CemClientes_CalculaTotalPagesCorreto()
    {
        // Arrange — 100 clientes com pageSize=50 → 2 páginas
        var clients = TestData.CreateClients(100);
        _serviceMock.Setup(s => s.GetAllClientsAsync(50, 0))
            .ReturnsAsync(new PaginatedResult<Client> { Items = clients.Take(50), TotalCount = 100, Limit = 50, Offset = 0 });

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.TotalPages.Should().Be(2);
    }

    #endregion

    #region AddClient — criação

    [Fact]
    public async Task AddClient_NomePreenchido_ChamaCreateERecarregaLista()
    {
        // Arrange
        _vm.Name = "João da Silva";
        _serviceMock.Setup(s => s.CreateClientAsync(It.IsAny<Client>())).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client>()));

        // Act
        await _vm.AddClientCommand.ExecuteAsync(null);

        // Assert
        _serviceMock.Verify(s => s.CreateClientAsync(It.Is<Client>(c => c.Name == "João da Silva")), Times.Once);
    }

    [Fact]
    public async Task AddClient_NomeVazio_NaoChamaCreate()
    {
        // Arrange
        _vm.Name = string.Empty;

        // Act
        await _vm.AddClientCommand.ExecuteAsync(null);

        // Assert
        _serviceMock.Verify(s => s.CreateClientAsync(It.IsAny<Client>()), Times.Never);
    }

    #endregion

    #region AddClient — edição

    [Fact]
    public async Task AddClient_ComClientIdPreenchido_ChamaUpdateEmVezDeCreate()
    {
        // Arrange
        var existingId = Guid.NewGuid();
        _vm.ClientId = existingId;
        _vm.Name = "Maria Atualizada";
        _serviceMock.Setup(s => s.UpdateClientAsync(It.IsAny<Client>())).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client>()));

        // Act
        await _vm.AddClientCommand.ExecuteAsync(null);

        // Assert
        _serviceMock.Verify(s => s.UpdateClientAsync(It.Is<Client>(c => c.ClientId == existingId && c.Name == "Maria Atualizada")), Times.Once);
        _serviceMock.Verify(s => s.CreateClientAsync(It.IsAny<Client>()), Times.Never);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_IdValido_ChamaDeleteERecarregaLista()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteClientAsync(clientId)).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client>()));

        // Act
        _vm.DeleteCommand.Execute(clientId);
        await Task.Delay(50); // aguarda async void

        // Assert
        _serviceMock.Verify(s => s.DeleteClientAsync(clientId), Times.Once);
    }

    #endregion

    #region OpenEdit

    [Fact]
    public async Task OpenEdit_ClienteExistente_PopulaFormulario()
    {
        // Arrange
        var client = TestData.CreateClient();
        client.Name = "Teste Edição";
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client> { client }));
        await _vm.LoadDataAsync();

        // Act
        _vm.OpenEditCommand.Execute(client.ClientId);

        // Assert
        _vm.ClientId.Should().Be(client.ClientId);
        _vm.Name.Should().Be("Teste Edição");
        _vm.IsEdit.Should().BeTrue();
    }

    [Fact]
    public void OpenEdit_ClienteInexistente_NaoAlteraEstado()
    {
        // Arrange — collection vazia
        var nomeAntes = _vm.Name;

        // Act
        _vm.OpenEditCommand.Execute(Guid.NewGuid());

        // Assert
        _vm.Name.Should().Be(nomeAntes);
        _vm.IsEdit.Should().BeFalse();
    }

    #endregion

    #region ClearForm

    [Fact]
    public void ClearForm_SempRe_ResetaEstado()
    {
        // Arrange
        _vm.Name = "Algum Nome";
        _vm.ClientId = Guid.NewGuid();
        _vm.IsEdit = true;

        // Act
        _vm.ClearFormCommand.Execute(null);

        // Assert
        _vm.Name.Should().BeEmpty();
        _vm.ClientId.Should().Be(Guid.Empty);
        _vm.IsEdit.Should().BeFalse();
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_TermoEncontrado_FiltraCollection()
    {
        // Arrange
        var c1 = TestData.CreateClient(); c1.Name = "Ana Paula";
        var c2 = TestData.CreateClient(); c2.Name = "Carlos Souza";
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client> { c1, c2 }));
        await _vm.LoadDataAsync();

        // Act
        _vm.SearchBarField = "Ana";
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Clients.Should().HaveCount(1);
        _vm.Clients[0].Name.Should().Be("Ana Paula");
    }

    [Fact]
    public async Task Search_TermoVazio_RestauraTodosOsClientes()
    {
        // Arrange
        var clients = TestData.CreateClients(3);
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(clients));
        await _vm.LoadDataAsync();
        _vm.SearchBarField = "xyz";
        _vm.SearchCommand.Execute(null);

        // Act
        _vm.SearchBarField = string.Empty;
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Clients.Should().HaveCount(3);
    }

    [Fact]
    public async Task Search_TermoComAcento_NormalizaEEncontra()
    {
        // Arrange
        var c = TestData.CreateClient(); c.Name = "José Ávila";
        _serviceMock.Setup(s => s.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Client> { c }));
        await _vm.LoadDataAsync();

        // Act
        _vm.SearchBarField = "jose avila";
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Clients.Should().HaveCount(1);
    }

    #endregion

    #region Paginação

    [Fact]
    public void CanPreviousPage_PrimeiraPagina_Falso()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages = 3;
        _vm.CanPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void CanNextPage_UltimaPagina_Falso()
    {
        _vm.CurrentPage = 3;
        _vm.TotalPages = 3;
        _vm.CanNextPage.Should().BeFalse();
    }

    [Fact]
    public void CanNextPage_NaoPrimeiraPagina_Verdadeiro()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages = 2;
        _vm.CanNextPage.Should().BeTrue();
    }

    #endregion
}
