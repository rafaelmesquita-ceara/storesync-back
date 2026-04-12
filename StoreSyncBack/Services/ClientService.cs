using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _repo;

        public ClientService(IClientRepository repo)
        {
            _repo = repo;
        }

        public Task<PaginatedResult<Client>> GetAllClientsAsync(int limit = 50, int offset = 0)
        {
            return _repo.GetAllClientsAsync(limit, offset);
        }

        public Task<Client?> GetClientByIdAsync(Guid clientId)
        {
            return _repo.GetClientByIdAsync(clientId);
        }

        public async Task<int> CreateClientAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(client.Name))
                throw new ArgumentException("Nome é obrigatório.", nameof(client.Name));

            if (client.ClientId == Guid.Empty)
                client.ClientId = Guid.NewGuid();

            if (client.CreatedAt == default)
                client.CreatedAt = BrazilDateTime.Now;

            return await _repo.CreateClientAsync(client);
        }

        public async Task<int> UpdateClientAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (client.ClientId == Guid.Empty)
                throw new ArgumentException("ClientId inválido.", nameof(client.ClientId));

            if (string.IsNullOrWhiteSpace(client.Name))
                throw new ArgumentException("Nome é obrigatório.", nameof(client.Name));

            return await _repo.UpdateClientAsync(client);
        }

        public Task<int> DeleteClientAsync(Guid clientId)
        {
            if (clientId == Guid.Empty)
                throw new ArgumentException("ClientId inválido.", nameof(clientId));

            return _repo.DeleteClientAsync(clientId);
        }
    }
}
