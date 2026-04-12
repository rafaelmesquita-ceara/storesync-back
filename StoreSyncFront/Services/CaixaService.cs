using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SharedModels;
using StoreSyncFront.Models;

namespace StoreSyncFront.Services;

public class CaixaService(IApiService apiService)
{
    public async Task<PaginatedResult<Caixa>> GetAllAsync(int limit, int offset)
    {
        Response response = await apiService.GetAsync($"/api/Caixa?limit={limit}&offset={offset}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<PaginatedResult<Caixa>>(response.Body) ?? new PaginatedResult<Caixa>();

        SnackBarService.SendError("Erro ao buscar caixas: " + response.Body);
        return new PaginatedResult<Caixa> { Items = new List<Caixa>() };
    }

    public async Task<Caixa?> GetByIdAsync(Guid id)
    {
        Response response = await apiService.GetAsync($"/api/Caixa/{id}");
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Caixa>(response.Body);

        SnackBarService.SendError("Erro ao buscar caixa: " + response.Body);
        return null;
    }

    public async Task<Caixa?> GetCaixaAbertoAsync()
    {
        Response response = await apiService.GetAsync("/api/Caixa/aberto");
        if (response.Status == 204)
            return null;
        if (response.IsSuccess())
            return JsonConvert.DeserializeObject<Caixa>(response.Body);

        return null;
    }

    public async Task<Caixa?> AbrirCaixaAsync(decimal valorAbertura)
    {
        var body = JsonContent.Create(new { ValorAbertura = valorAbertura });
        Response response = await apiService.PostAsync("/api/Caixa", body);
        if (response.IsSuccess())
        {
            SnackBarService.SendSuccess("Caixa aberto com sucesso.");
            return JsonConvert.DeserializeObject<Caixa>(response.Body);
        }

        SnackBarService.SendError("Erro ao abrir caixa: " + response.Body);
        return null;
    }

    public async Task<bool> FecharCaixaAsync(Guid id, decimal valorFechamento)
    {
        var body = JsonContent.Create(new { ValorFechamento = valorFechamento });
        Response response = await apiService.PostAsync($"/api/Caixa/{id}/fechar", body);
        if (response.IsSuccess())
        {
            SnackBarService.SendSuccess("Caixa fechado com sucesso.");
            return true;
        }

        SnackBarService.SendError("Erro ao fechar caixa: " + response.Body);
        return false;
    }

    public async Task<bool> AddMovimentacaoAsync(Guid caixaId, int tipo, string? descricao, decimal valor)
    {
        var body = JsonContent.Create(new { Tipo = tipo, Descricao = descricao, Valor = valor });
        Response response = await apiService.PostAsync($"/api/Caixa/{caixaId}/movimentacao", body);
        if (response.IsSuccess())
        {
            SnackBarService.SendSuccess("Movimentação registrada com sucesso.");
            return true;
        }

        SnackBarService.SendError("Erro ao registrar movimentação: " + response.Body);
        return false;
    }

    public async Task<byte[]?> DownloadCaixaReportAsync(Guid caixaId)
    {
        var bytes = await apiService.DownloadAsync($"/api/Caixa/{caixaId}/report/pdf");
        if (bytes == null)
            SnackBarService.SendError("Erro ao gerar relatório do caixa.");
        return bytes;
    }
}
