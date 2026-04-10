using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class FinanceService(IApiService apiService) : IFinanceService
{
    public async Task<IEnumerable<Finance>> GetAllFinanceAsync()
    {
        Response response = await apiService.GetAsync("/api/Finance");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<IEnumerable<Finance>>(response.Body) ?? new List<Finance>();

        SnackBarService.Send("Erro ao buscar registros financeiros: " + response.Body);
        return new List<Finance>();
    }

    public async Task<IEnumerable<Finance>> GetAllByTypeAsync(int type)
    {
        Response response = await apiService.GetAsync($"/api/Finance?type={type}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<IEnumerable<Finance>>(response.Body) ?? new List<Finance>();

        SnackBarService.Send("Erro ao buscar registros financeiros: " + response.Body);
        return new List<Finance>();
    }

    public async Task<Finance?> GetFinanceByIdAsync(Guid financeId)
    {
        Response response = await apiService.GetAsync($"/api/Finance/{financeId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Finance>(response.Body);

        SnackBarService.Send("Erro ao buscar o registro financeiro: " + response.Body);
        return null;
    }

    public async Task<int> CreateFinanceAsync(Finance finance)
    {
        Response response = await apiService.PostAsync("/api/Finance", JsonContent.Create(finance));
        SnackBarService.Send(response.IsSuccess()
            ? "Registro financeiro criado com sucesso."
            : "Erro ao criar registro financeiro: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> UpdateFinanceAsync(Finance finance)
    {
        Response response = await apiService.PutAsync($"/api/Finance/{finance.FinanceId}", JsonContent.Create(finance));
        SnackBarService.Send(response.IsSuccess()
            ? "Registro financeiro atualizado com sucesso."
            : "Erro ao atualizar registro financeiro: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task<int> DeleteFinanceAsync(Guid financeId)
    {
        Response response = await apiService.DeleteAsync($"/api/Finance/{financeId}");
        SnackBarService.Send(response.IsSuccess()
            ? "Registro financeiro excluído com sucesso."
            : "Erro ao excluir registro financeiro: " + response.Body);
        return response.IsSuccess() ? 0 : 1;
    }

    public async Task SettleAsync(Guid financeId, decimal settledAmount, string? note)
    {
        var body = new { settledAmount, note };
        Response response = await apiService.PostAsync($"/api/Finance/{financeId}/settle", JsonContent.Create(body));
        SnackBarService.Send(response.IsSuccess()
            ? "Título liquidado com sucesso."
            : "Erro ao liquidar título: " + response.Body);
    }

    public async Task CancelSettlementAsync(Guid financeId)
    {
        Response response = await apiService.DeleteAsync($"/api/Finance/{financeId}/settle");
        SnackBarService.Send(response.IsSuccess()
            ? "Liquidação cancelada com sucesso."
            : "Erro ao cancelar liquidação: " + response.Body);
    }
}
