namespace SharedModels.Interfaces;

public interface IClientRepository
{
    Task<PaginatedResult<Client>> GetAllClientsAsync(int limit = 50, int offset = 0);
    Task<Client?> GetClientByIdAsync(Guid clientId);
    Task<int> CreateClientAsync(Client client);
    Task<int> UpdateClientAsync(Client client);
    Task<int> DeleteClientAsync(Guid clientId);
}

public interface IClientService
{
    Task<PaginatedResult<Client>> GetAllClientsAsync(int limit = 50, int offset = 0);
    Task<Client?> GetClientByIdAsync(Guid clientId);
    Task<int> CreateClientAsync(Client client);
    Task<int> UpdateClientAsync(Client client);
    Task<int> DeleteClientAsync(Guid clientId);
}
