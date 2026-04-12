using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class ClientService(IApiService apiService) : IClientService
{
    public async Task<PaginatedResult<Client>> GetAllClientsAsync(int limit = 50, int offset = 0)
    {
        Response response = await apiService.GetAsync($"/api/clients?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<Client>>(response.Body) ?? new PaginatedResult<Client>();

        SnackBarService.Send("Erro ao buscar clientes: " + response.Body);
        return new PaginatedResult<Client> { Items = new List<Client>() };
    }

    public async Task<Client?> GetClientByIdAsync(Guid clientId)
    {
        Response response = await apiService.GetAsync($"/api/clients/{clientId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Client>(response.Body);

        SnackBarService.Send("Erro ao buscar cliente: " + response.Body);
        return null;
    }

    public async Task<int> CreateClientAsync(Client client)
    {
        Response response = await apiService.PostAsync("/api/clients", JsonContent.Create(client));
        SnackBarService.Send(response.IsSuccess()
            ? "Cliente cadastrado com sucesso."
            : "Erro ao cadastrar cliente: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> UpdateClientAsync(Client client)
    {
        Response response = await apiService.PutAsync($"/api/clients/{client.ClientId}", JsonContent.Create(client));
        SnackBarService.Send(response.IsSuccess()
            ? "Cliente atualizado com sucesso."
            : "Erro ao atualizar cliente: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> DeleteClientAsync(Guid clientId)
    {
        Response response = await apiService.DeleteAsync($"/api/clients/{clientId}");
        SnackBarService.Send(response.IsSuccess()
            ? "Cliente excluído com sucesso."
            : "Erro ao excluir cliente: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }
}
