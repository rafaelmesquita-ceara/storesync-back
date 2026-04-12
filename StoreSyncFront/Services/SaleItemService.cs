using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class SaleItemService(IApiService apiService) : ISaleItemService
{
    public async Task<PaginatedResult<SaleItem>> GetAllSaleItemsAsync(int limit = 50, int offset = 0)
    {
        Response response = await apiService.GetAsync($"/api/SaleItems?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<SaleItem>>(response.Body) ?? new PaginatedResult<SaleItem>();

        SnackBarService.Send("Erro ao buscar itens de venda: " + response.Body);
        return new PaginatedResult<SaleItem> { Items = new List<SaleItem>() };
    }

    public async Task<PaginatedResult<SaleItem>> GetSaleItemsBySaleIdAsync(Guid saleId, int limit = 50, int offset = 0)
    {
        Response response = await apiService.GetAsync($"/api/SaleItems/by-sale/{saleId}?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<SaleItem>>(response.Body) ?? new PaginatedResult<SaleItem>();

        SnackBarService.Send("Erro ao buscar itens da venda: " + response.Body);
        return new PaginatedResult<SaleItem> { Items = new List<SaleItem>() };
    }

    public async Task<SaleItem?> GetSaleItemByIdAsync(Guid saleItemId)
    {
        Response response = await apiService.GetAsync($"/api/SaleItems/{saleItemId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<SaleItem>(response.Body);

        SnackBarService.Send("Erro ao buscar item da venda: " + response.Body);
        return null;
    }

    public async Task<int> CreateSaleItemAsync(SaleItem saleItem)
    {
        Response response = await apiService.PostAsync("/api/SaleItems", JsonContent.Create(saleItem));
        if (response.IsSuccess())
        {
            var created = JsonConvert.DeserializeObject<SaleItem>(response.Body);
            if (created != null)
                saleItem.SaleItemId = created.SaleItemId;
            return 0;
        }

        SnackBarService.Send("Erro ao adicionar item: " + response.Body);
        return 1;
    }

    public async Task<int> UpdateSaleItemAsync(SaleItem saleItem)
    {
        Response response = await apiService.PutAsync($"/api/SaleItems/{saleItem.SaleItemId}", JsonContent.Create(saleItem));
        if (response.IsSuccess())
            return 0;

        SnackBarService.Send("Erro ao atualizar item: " + response.Body);
        return 1;
    }

    public async Task<int> DeleteSaleItemAsync(Guid saleItemId)
    {
        Response response = await apiService.DeleteAsync($"/api/SaleItems/{saleItemId}");
        if (response.IsSuccess())
            return 0;

        SnackBarService.Send("Erro ao remover item: " + response.Body);
        return 1;
    }
}
