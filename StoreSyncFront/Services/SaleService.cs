using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class SaleService(IApiService apiService) : ISaleService
{
    public async Task<PaginatedResult<Sale>> GetAllSalesAsync(int limit = 50, int offset = 0)
    {
        Response response = await apiService.GetAsync($"/api/Sales?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<Sale>>(response.Body) ?? new PaginatedResult<Sale>();

        SnackBarService.SendError("Erro ao buscar vendas: " + response.Body);
        return new PaginatedResult<Sale> { Items = new List<Sale>() };
    }

    public async Task<Sale?> GetSaleByIdAsync(Guid saleId)
    {
        Response response = await apiService.GetAsync($"/api/Sales/{saleId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Sale>(response.Body);

        SnackBarService.SendError("Erro ao buscar venda: " + response.Body);
        return null;
    }

    public async Task<int> CreateSaleAsync(Sale sale)
    {
        Response response = await apiService.PostAsync("/api/Sales", JsonContent.Create(sale));
        if (response.IsSuccess())
        {
            var created = JsonConvert.DeserializeObject<Sale>(response.Body);
            if (created != null)
                sale.SaleId = created.SaleId;
            return 0;
        }

        SnackBarService.SendError("Erro ao criar venda: " + response.Body);
        return 1;
    }

    public async Task<int> UpdateSaleAsync(Sale sale)
    {
        Response response = await apiService.PutAsync($"/api/Sales/{sale.SaleId}", JsonContent.Create(sale));
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao atualizar venda: " + response.Body);
        return 1;
    }

    public async Task<int> FinalizeSaleAsync(Guid saleId)
    {
        Response response = await apiService.PostAsync($"/api/Sales/{saleId}/finalize", null);
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Venda finalizada com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao finalizar venda: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> CancelSaleAsync(Guid saleId)
    {
        Response response = await apiService.PostAsync($"/api/Sales/{saleId}/cancel", null);
        if (response.IsSuccess())
            SnackBarService.SendSuccess("Venda cancelada com sucesso."
            );
        else
            SnackBarService.SendError("Erro ao cancelar venda: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<byte[]?> DownloadSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var bytes = await apiService.DownloadAsync($"/api/Sales/report/pdf?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        if (bytes == null)
        {
            SnackBarService.SendError("Erro ao gerar relatório de vendas.");
        }
        return bytes;
    }
}
