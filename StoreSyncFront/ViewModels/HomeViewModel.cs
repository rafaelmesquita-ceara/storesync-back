using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SharedModels;
using SharedModels.Interfaces;
using SkiaSharp;

namespace StoreSyncFront.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ISaleService _saleService;
    private readonly IFinanceService _financeService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IEmployeeService _employeeService;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private bool _isLoading;

    // KPIs
    [ObservableProperty] private int _totalVendasMes;
    [ObservableProperty] private decimal _faturamentoMes;
    [ObservableProperty] private decimal _aReceberAberto;
    [ObservableProperty] private decimal _aPagarAberto;

    // Gráfico 1 — Vendas por Dia (Linha)
    [ObservableProperty] private ISeries[] _vendasPorDiaSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _vendasPorDiaXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _vendasPorDiaYAxes = Array.Empty<Axis>();

    // Gráfico 2 — Financeiro A Pagar vs A Receber (Barras agrupadas)
    [ObservableProperty] private ISeries[] _financeiroPagarReceberSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _financeiroPagarReceberXAxes = Array.Empty<Axis>();

    // Gráfico 3 — Vendas por Funcionário (Barras horizontais)
    [ObservableProperty] private ISeries[] _vendasPorFuncionarioSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _vendasPorFuncionarioXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _vendasPorFuncionarioYAxes = Array.Empty<Axis>();

    // Gráfico 4 — Estoque por Categoria (Rosca)
    [ObservableProperty] private IEnumerable<ISeries> _estoquePorCategoriaSeries = Array.Empty<ISeries>();

    // Gráfico 5 — Status das Contas (Rosca)
    [ObservableProperty] private IEnumerable<ISeries> _statusContasSeries = Array.Empty<ISeries>();

    public HomeViewModel(
        string username,
        ISaleService saleService,
        IFinanceService financeService,
        IProductService productService,
        ICategoryService categoryService,
        IEmployeeService employeeService)
    {
        _username = username;
        _saleService = saleService;
        _financeService = financeService;
        _productService = productService;
        _categoryService = categoryService;
        _employeeService = employeeService;
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var salesTask      = _saleService.GetAllSalesAsync();
            var financeTask    = _financeService.GetAllFinanceAsync();
            var productsTask   = _productService.GetAllProductsAsync();
            var categoriesTask = _categoryService.GetAllCategoriesAsync();
            var employeesTask  = _employeeService.GetAllEmployeesAsync();

            await Task.WhenAll(salesTask, financeTask, productsTask, categoriesTask, employeesTask);

            var sales      = (await salesTask).Items.ToList();
            var finances   = (await financeTask).Items.ToList();
            var products   = (await productsTask).Items.ToList();
            var categories = (await categoriesTask).Items.ToList();
            var employees  = (await employeesTask).Items.ToList();

            BuildKpis(sales, finances);
            BuildVendasPorDia(sales);
            BuildFinanceiroPagarReceber(finances);
            BuildVendasPorFuncionario(sales, employees);
            BuildEstoquePorCategoria(products, categories);
            BuildStatusContas(finances);
        }
        catch
        {
            // mantém gráficos vazios sem derrubar a aplicação
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadDataAsync();
    }

    private void BuildKpis(List<Sale> sales, List<Finance> finances)
    {
        var now = BrazilDateTime.Now;
        var vendsMes = sales.Where(s =>
            s.Status == SaleStatus.Finalizada &&
            s.SaleDate.Year == now.Year &&
            s.SaleDate.Month == now.Month).ToList();

        TotalVendasMes = vendsMes.Count;
        FaturamentoMes = vendsMes.Sum(s => s.TotalAmount);
        AReceberAberto = finances
            .Where(f => f.Type == FinanceType.Receber && f.Status == FinanceStatus.Aberto)
            .Sum(f => f.Amount);
        APagarAberto = finances
            .Where(f => f.Type == FinanceType.Pagar && f.Status == FinanceStatus.Aberto)
            .Sum(f => f.Amount);
    }

    private void BuildVendasPorDia(List<Sale> sales)
    {
        var today = BrazilDateTime.Now.Date;
        var start = today.AddDays(-29);
        var labels = new string[30];
        var values = new double[30];

        for (int i = 0; i < 30; i++)
        {
            var day = start.AddDays(i);
            labels[i] = day.ToString("dd/MM");
            values[i] = (double)sales
                .Where(s => s.Status == SaleStatus.Finalizada && s.SaleDate.Date == day)
                .Sum(s => s.TotalAmount);
        }

        VendasPorDiaSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Vendas (R$)",
                Values = values,
                Fill = null,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(255, 193, 7)) { StrokeThickness = 2 },
                GeometryStroke = new SolidColorPaint(new SKColor(255, 193, 7)),
                GeometryFill = new SolidColorPaint(new SKColor(255, 193, 7)),
            }
        };

        VendasPorDiaXAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = 45,
                TextSize = 10,
            }
        };

        VendasPorDiaYAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => value.ToString("C0", CultureInfo.GetCultureInfo("pt-BR")),
                TextSize = 10,
            }
        };
    }

    private void BuildFinanceiroPagarReceber(List<Finance> finances)
    {
        var statusLabels = new[] { "Aberto", "Liquidado", "Liq. Parcial" };
        var pagarValues   = new double[3];
        var receberValues = new double[3];

        var statusIdx = new Dictionary<int, int>
        {
            { FinanceStatus.Aberto, 0 },
            { FinanceStatus.Liquidado, 1 },
            { FinanceStatus.LiquidadoParcialmente, 2 }
        };

        foreach (var f in finances)
        {
            if (!statusIdx.TryGetValue(f.Status, out int idx)) continue;
            if (f.Type == FinanceType.Pagar)   pagarValues[idx]   += (double)f.Amount;
            if (f.Type == FinanceType.Receber) receberValues[idx] += (double)f.Amount;
        }

        FinanceiroPagarReceberSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name   = "A Pagar",
                Values = pagarValues,
                Fill   = new SolidColorPaint(new SKColor(244, 67, 54)),
                MaxBarWidth = 40,
            },
            new ColumnSeries<double>
            {
                Name   = "A Receber",
                Values = receberValues,
                Fill   = new SolidColorPaint(new SKColor(76, 175, 80)),
                MaxBarWidth = 40,
            }
        };

        FinanceiroPagarReceberXAxes = new Axis[]
        {
            new Axis { Labels = statusLabels, TextSize = 11 }
        };
    }

    private void BuildVendasPorFuncionario(List<Sale> sales, List<Employee> employees)
    {
        var empDict = employees.ToDictionary(e => e.EmployeeId, e => e.Name ?? "?");

        var grouped = sales
            .Where(s => s.Status == SaleStatus.Finalizada)
            .GroupBy(s => s.EmployeeId)
            .Select(g => new
            {
                Name  = empDict.GetValueOrDefault(g.Key, "?"),
                Total = (double)g.Sum(s => s.TotalAmount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var names  = grouped.Select(g => g.Name).ToArray();
        var values = grouped.Select(g => g.Total).ToArray();

        VendasPorFuncionarioSeries = new ISeries[]
        {
            new RowSeries<double>
            {
                Name        = "Total (R$)",
                Values      = values,
                Fill        = new SolidColorPaint(new SKColor(33, 150, 243)),
                MaxBarWidth = 20,
            }
        };

        VendasPorFuncionarioYAxes = new Axis[]
        {
            new Axis 
            { 
                Labels = names, 
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(220, 220, 220))
            }
        };

        VendasPorFuncionarioXAxes = new Axis[]
        {
            new Axis 
            { 
                Labeler = value => value.ToString("C0", CultureInfo.GetCultureInfo("pt-BR")), 
                TextSize = 10,
                LabelsPaint = new SolidColorPaint(new SKColor(220, 220, 220))
            }
        };
    }

    private void BuildEstoquePorCategoria(List<Product> products, List<Category> categories)
    {
        var catDict = categories.ToDictionary(c => c.CategoryId, c => c.Name ?? "Sem Categoria");

        var grouped = products
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                Name  = g.Key.HasValue && catDict.TryGetValue(g.Key.Value, out var n) ? n : "Sem Categoria",
                Total = (double)g.Sum(p => p.StockQuantity)
            })
            .Where(g => g.Total > 0)
            .ToList();

        EstoquePorCategoriaSeries = grouped.Select(g =>
            (ISeries)new PieSeries<double>
            {
                Name        = g.Name,
                Values      = new double[] { g.Total },
                InnerRadius = 50,
            }
        ).ToArray();
    }

    private void BuildStatusContas(List<Finance> finances)
    {
        var statusLabels = new Dictionary<int, string>
        {
            { FinanceStatus.Aberto,                "Aberto" },
            { FinanceStatus.Liquidado,             "Liquidado" },
            { FinanceStatus.LiquidadoParcialmente, "Liq. Parcial" }
        };

        StatusContasSeries = statusLabels
            .Select(kv => (ISeries)new PieSeries<double>
            {
                Name        = kv.Value,
                Values      = new double[] { finances.Count(f => f.Status == kv.Key) },
                InnerRadius = 60,
            })
            .ToArray();
    }
}
