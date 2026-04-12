using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedModels;
using StoreSyncFront.Services;

namespace StoreSyncFront.ViewModels;

public partial class CaixasViewModel : ObservableObject
{
    private readonly CaixaService _caixaService;

    public ObservableCollection<Caixa> Caixas { get; } = new();
    public ObservableCollection<Sale> Vendas { get; } = new();
    public ObservableCollection<MovimentacaoCaixa> Movimentacoes { get; } = new();
    private List<Caixa>? _allCaixas;

    [ObservableProperty] private string _searchBarField = string.Empty;
    [ObservableProperty] private bool _isEdit;
    [ObservableProperty] private bool _isActionsExpanded;

    // Paginação (modo lista)
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    private readonly int _pageSize = 50;

    public bool CanPreviousPage => CurrentPage > 1;
    public bool CanNextPage => CurrentPage < TotalPages;

    // Dados do caixa selecionado (modo formulário)
    [ObservableProperty] private Guid _caixaId;
    [ObservableProperty] private string _referencia = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private decimal _valorAbertura;
    [ObservableProperty] private decimal? _valorFechamento;
    [ObservableProperty] private decimal _totalVendas;
    [ObservableProperty] private decimal _totalSangrias;
    [ObservableProperty] private decimal _totalSuprimentos;
    [ObservableProperty] private decimal? _valorFaltante;
    [ObservableProperty] private decimal? _valorSobra;
    [ObservableProperty] private DateTime _dataAbertura;
    [ObservableProperty] private DateTime? _dataFechamento;

    public bool IsCaixaAberto => StatusLabel == "Aberto";

    public CaixasViewModel(CaixaService caixaService)
    {
        _caixaService = caixaService;
    }

    [RelayCommand(CanExecute = nameof(CanPreviousPage))]
    private async Task PreviousPage()
    {
        CurrentPage--;
        await LoadDataAsync();
    }

    [RelayCommand(CanExecute = nameof(CanNextPage))]
    private async Task NextPage()
    {
        CurrentPage++;
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        var offset = (CurrentPage - 1) * _pageSize;
        var result = await _caixaService.GetAllAsync(_pageSize, offset);

        TotalCount = result.TotalCount;
        TotalPages = (int)Math.Ceiling((double)TotalCount / _pageSize);
        if (TotalPages == 0) TotalPages = 1;

        Caixas.Clear();
        foreach (var c in result.Items)
            Caixas.Add(c);

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();

        _allCaixas = Caixas.ToList();
    }

    public async Task AbrirFormulario(Guid caixaId)
    {
        var caixa = await _caixaService.GetByIdAsync(caixaId);
        if (caixa == null) return;

        CaixaId = caixa.CaixaId;
        Referencia = caixa.Referencia;
        StatusLabel = caixa.StatusLabel;
        ValorAbertura = caixa.ValorAbertura;
        ValorFechamento = caixa.ValorFechamento;
        TotalVendas = caixa.TotalVendas;
        TotalSangrias = caixa.TotalSangrias;
        TotalSuprimentos = caixa.TotalSuprimentos;
        ValorFaltante = caixa.ValorFaltante;
        ValorSobra = caixa.ValorSobra;
        DataAbertura = caixa.DataAbertura;
        DataFechamento = caixa.DataFechamento;

        Vendas.Clear();
        if (caixa.Vendas != null)
            foreach (var v in caixa.Vendas)
                Vendas.Add(v);

        Movimentacoes.Clear();
        if (caixa.Movimentacoes != null)
            foreach (var m in caixa.Movimentacoes)
                Movimentacoes.Add(m);

        OnPropertyChanged(nameof(IsCaixaAberto));
        IsActionsExpanded = false;
        IsEdit = true;
    }

    [RelayCommand]
    public void ToggleActions() => IsActionsExpanded = !IsActionsExpanded;

    [RelayCommand]
    public void VoltarLista()
    {
        IsEdit = false;
        IsActionsExpanded = false;
    }

    [RelayCommand]
    public async Task Refresh()
    {
        if (IsEdit)
            await AbrirFormulario(CaixaId);
        else
            await LoadDataAsync();
    }

    [RelayCommand]
    public void Search()
    {
        if (_allCaixas == null) _allCaixas = Caixas.ToList();

        var query = (SearchBarField ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Caixas.Clear();
            foreach (var c in _allCaixas) Caixas.Add(c);
            return;
        }

        var tokens = Regex.Split(query, @"\s+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(Normalize)
            .ToArray();

        var filtered = _allCaixas.Where(c =>
        {
            var combined = $"{c.Referencia} {c.StatusLabel}";
            var norm = Normalize(combined);
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        Caixas.Clear();
        foreach (var c in filtered) Caixas.Add(c);
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    // Chamado pelo code-behind com o resultado do FecharCaixaDialog
    public async Task<bool> FecharCaixa(decimal valorFechamento)
    {
        var ok = await _caixaService.FecharCaixaAsync(CaixaId, valorFechamento);
        if (ok)
        {
            await AbrirFormulario(CaixaId);
        }
        return ok;
    }

    // Chamado pelo code-behind com o resultado do AddMovimentacaoCaixaDialog
    public async Task<bool> AddMovimentacao(int tipo, string? descricao, decimal valor)
    {
        var ok = await _caixaService.AddMovimentacaoAsync(CaixaId, tipo, descricao, valor);
        if (ok)
        {
            await AbrirFormulario(CaixaId);
        }
        return ok;
    }

    // Chamado pelo code-behind para exportar PDF
    public Task<byte[]?> DownloadRelatorio() => _caixaService.DownloadCaixaReportAsync(CaixaId);
}
