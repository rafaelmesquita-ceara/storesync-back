using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class PaymentMethodService(IApiService apiService) : IPaymentMethodService
{
    public async Task<IEnumerable<PaymentMethod>> GetAllAsync()
    {
        Response response = await apiService.GetAsync("/api/PaymentMethods");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<List<PaymentMethod>>(response.Body) ?? new List<PaymentMethod>();

        SnackBarService.SendError("Erro ao buscar formas de pagamento: " + response.Body);
        return new List<PaymentMethod>();
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id)
    {
        Response response = await apiService.GetAsync($"/api/PaymentMethods/{id}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaymentMethod>(response.Body);

        SnackBarService.SendError("Erro ao buscar forma de pagamento: " + response.Body);
        return null;
    }

    public async Task<int> CreateAsync(PaymentMethod pm)
    {
        Response response = await apiService.PostAsync("/api/PaymentMethods", JsonContent.Create(pm));
        if (response.IsSuccess())
        {
            var created = JsonConvert.DeserializeObject<PaymentMethod>(response.Body);
            if (created != null)
                pm.PaymentMethodId = created.PaymentMethodId;
            return 0;
        }

        SnackBarService.SendError("Erro ao criar forma de pagamento: " + response.Body);
        return 1;
    }

    public async Task<int> UpdateAsync(PaymentMethod pm)
    {
        Response response = await apiService.PutAsync($"/api/PaymentMethods/{pm.PaymentMethodId}", JsonContent.Create(pm));
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao atualizar forma de pagamento: " + response.Body);
        return 1;
    }

    public async Task<int> DeleteAsync(Guid id)
    {
        Response response = await apiService.DeleteAsync($"/api/PaymentMethods/{id}");
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao excluir forma de pagamento: " + response.Body);
        return 1;
    }

    public async Task<int> AddRateAsync(Guid methodId, PaymentMethodRate rate)
    {
        Response response = await apiService.PostAsync($"/api/PaymentMethods/{methodId}/rates", JsonContent.Create(rate));
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao adicionar taxa: " + response.Body);
        return 1;
    }

    public async Task<int> DeleteRateAsync(Guid methodId, Guid rateId)
    {
        Response response = await apiService.DeleteAsync($"/api/PaymentMethods/{methodId}/rates/{rateId}");
        if (response.IsSuccess())
            return 0;

        SnackBarService.SendError("Erro ao remover taxa: " + response.Body);
        return 1;
    }
}
