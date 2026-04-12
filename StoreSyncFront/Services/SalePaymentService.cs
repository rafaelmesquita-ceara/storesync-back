using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class SalePaymentService(IApiService apiService) : ISalePaymentService
{
    public async Task<IEnumerable<SalePayment>> GetBySaleIdAsync(Guid saleId)
    {
        Response response = await apiService.GetAsync($"/api/SalePayments/by-sale/{saleId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<List<SalePayment>>(response.Body) ?? new List<SalePayment>();

        SnackBarService.SendError("Erro ao buscar pagamentos: " + response.Body);
        return new List<SalePayment>();
    }

    public async Task<int> AddPaymentAsync(SalePayment payment)
    {
        Response response = await apiService.PostAsync("/api/SalePayments", JsonContent.Create(payment));
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao registrar pagamento: " + response.Body);
        return 1;
    }

    public async Task<int> RemovePaymentAsync(Guid salePaymentId)
    {
        Response response = await apiService.DeleteAsync($"/api/SalePayments/{salePaymentId}");
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao remover pagamento: " + response.Body);
        return 1;
    }
}
