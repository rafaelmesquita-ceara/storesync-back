using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class CommissionService(IApiService apiService) : ICommissionService
{
    public async Task<IEnumerable<Commission>> GetAllCommissionsAsync()
    {
        Response response = await apiService.GetAsync("/api/Commissions");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<IEnumerable<Commission>>(response.Body) ?? new List<Commission>();

        SnackBarService.Send("Erro ao buscar comissões: " + response.Body);
        return new List<Commission>();
    }

    public async Task<Commission?> GetCommissionByIdAsync(Guid commissionId)
    {
        Response response = await apiService.GetAsync($"/api/Commissions/{commissionId}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Commission>(response.Body);

        SnackBarService.Send("Erro ao buscar comissão: " + response.Body);
        return null;
    }

    public async Task<(decimal TotalSales, decimal CommissionRate, decimal CommissionValue)> CalculateAsync(
        Guid employeeId, DateTime startDate, DateTime endDate)
    {
        var url = $"/api/Commissions/calculate?employeeId={employeeId}" +
                  $"&startDate={startDate:yyyy-MM-dd}" +
                  $"&endDate={endDate:yyyy-MM-dd}";

        Response response = await apiService.GetAsync(url);
        if (!response.IsSuccess())
        {
            SnackBarService.Send("Erro ao calcular comissão: " + response.Body);
            return (0, 0, 0);
        }

        var obj = JObject.Parse(response.Body);
        var totalSales      = obj["totalSales"]?.Value<decimal>() ?? 0;
        var commissionRate  = obj["commissionRate"]?.Value<decimal>() ?? 0;
        var commissionValue = obj["commissionValue"]?.Value<decimal>() ?? 0;
        return (totalSales, commissionRate, commissionValue);
    }

    public async Task<int> CreateCommissionAsync(Commission commission)
    {
        Response response = await apiService.PostAsync("/api/Commissions", JsonContent.Create(commission));
        if (response.IsSuccess())
        {
            SnackBarService.Send("Comissão criada com sucesso.");
            return 0;
        }

        SnackBarService.Send("Erro ao criar comissão: " + response.Body);
        return 1;
    }

    public async Task<int> DeleteCommissionAsync(Guid commissionId)
    {
        Response response = await apiService.DeleteAsync($"/api/Commissions/{commissionId}");
        if (response.IsSuccess())
        {
            SnackBarService.Send("Comissão excluída com sucesso.");
            return 0;
        }

        SnackBarService.Send("Erro ao excluir comissão: " + response.Body);
        return 1;
    }
}
