using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;
using StoreSyncBack.Tests.Fixtures;
using Xunit;

namespace StoreSyncBack.Tests.Unit.Services
{
    public class ClientServiceTests
    {
        private readonly Mock<IClientRepository> _clientRepoMock;
        private readonly ClientService _clientService;

        public ClientServiceTests()
        {
            _clientRepoMock = new Mock<IClientRepository>();
            _clientService = new ClientService(_clientRepoMock.Object);
        }

        #region GetAllClientsAsync

        [Fact]
        public async Task GetAllClientsAsync_ClientesExistem_RetornaListaDePaginada()
        {
            // Arrange
            var expectedClients = TestData.CreateClients(5);
            var paginated = new PaginatedResult<Client> { Items = expectedClients, TotalCount = 5 };
            _clientRepoMock.Setup(r => r.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginated);

            // Act
            var result = await _clientService.GetAllClientsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(5);
        }

        #endregion

        #region GetClientByIdAsync

        [Fact]
        public async Task GetClientByIdAsync_ClienteExistente_RetornaCliente()
        {
            // Arrange
            var expected = TestData.CreateClient();
            _clientRepoMock.Setup(r => r.GetClientByIdAsync(expected.ClientId))
                .ReturnsAsync(expected);

            // Act
            var result = await _clientService.GetClientByIdAsync(expected.ClientId);

            // Assert
            result.Should().NotBeNull();
            result!.ClientId.Should().Be(expected.ClientId);
        }

        [Fact]
        public async Task GetClientByIdAsync_ClienteInexistente_RetornaNull()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            _clientRepoMock.Setup(r => r.GetClientByIdAsync(clientId))
                .ReturnsAsync((Client?)null);

            // Act
            var result = await _clientService.GetClientByIdAsync(clientId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateClientAsync

        [Fact]
        public async Task CreateClientAsync_ClienteValido_RetornaUm()
        {
            // Arrange
            var client = TestData.CreateClient();
            _clientRepoMock.Setup(r => r.CreateClientAsync(It.IsAny<Client>()))
                .ReturnsAsync(1);

            // Act
            var result = await _clientService.CreateClientAsync(client);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task CreateClientAsync_ClienteNull_LancaArgumentNullException()
        {
            // Arrange
            Client? client = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _clientService.CreateClientAsync(client!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateClientAsync_NomeVazio_LancaArgumentException(string? name)
        {
            // Arrange
            var client = TestData.CreateClient();
            client.Name = name ?? "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _clientService.CreateClientAsync(client));
        }

        [Fact]
        public async Task CreateClientAsync_SemCpfCnpj_CriaClienteComSucesso()
        {
            // Arrange — cpf_cnpj é opcional
            var client = TestData.CreateClient();
            client.CpfCnpj = null;
            _clientRepoMock.Setup(r => r.CreateClientAsync(It.IsAny<Client>()))
                .ReturnsAsync(1);

            // Act
            var result = await _clientService.CreateClientAsync(client);

            // Assert
            result.Should().Be(1);
        }

        #endregion

        #region UpdateClientAsync

        [Fact]
        public async Task UpdateClientAsync_ClienteValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var client = TestData.CreateClient();
            _clientRepoMock.Setup(r => r.UpdateClientAsync(It.IsAny<Client>()))
                .ReturnsAsync(1);

            // Act
            var result = await _clientService.UpdateClientAsync(client);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateClientAsync_ClienteNull_LancaArgumentNullException()
        {
            // Arrange
            Client? client = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _clientService.UpdateClientAsync(client!));
        }

        [Fact]
        public async Task UpdateClientAsync_ClienteIdVazio_LancaArgumentException()
        {
            // Arrange
            var client = TestData.CreateClient();
            client.ClientId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _clientService.UpdateClientAsync(client));
        }

        [Fact]
        public async Task UpdateClientAsync_NomeVazio_LancaArgumentException()
        {
            // Arrange
            var client = TestData.CreateClient();
            client.Name = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _clientService.UpdateClientAsync(client));
        }

        #endregion

        #region DeleteClientAsync

        [Fact]
        public async Task DeleteClientAsync_IdValido_RetornaLinhasAfetadas()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            _clientRepoMock.Setup(r => r.DeleteClientAsync(clientId))
                .ReturnsAsync(1);

            // Act
            var result = await _clientService.DeleteClientAsync(clientId);

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteClientAsync_IdVazio_LancaArgumentException()
        {
            // Arrange
            var clientId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _clientService.DeleteClientAsync(clientId));
        }

        #endregion
    }
}
