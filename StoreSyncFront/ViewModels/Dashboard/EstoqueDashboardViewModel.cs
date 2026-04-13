using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SharedModels;
using SkiaSharp;

namespace StoreSyncFront.ViewModels.Dashboard;

public partial class EstoqueDashboardViewModel : DashboardPageViewModelBase
{
    // KPIs
    [ObservableProperty] private int _totalProdutos;
    [ObservableProperty] private int _produtosEstoqueZero;
    [ObservableProperty] private decimal _valorEstoqueCusto;
    [ObservableProperty] private decimal _valorEstoqueVenda;

    // Gráfico — Estoque por Categoria (Rosca)
    [ObservableProperty] private IEnumerable<ISeries> _estoquePorCategoriaSeries = Array.Empty<ISeries>();

    // Gráfico — Top 10 Produtos Mais Vendidos (Barras horizontais)
    [ObservableProperty] private ISeries[] _topProdutosSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _topProdutosXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _topProdutosYAxes = Array.Empty<Axis>();

    public EstoqueDashboardViewModel()
    {
        Title = "Estoque";
    }

    public override void BuildFromData(DashboardDataBundle bundle)
    {
        BuildKpis(bundle.Products);
        BuildEstoquePorCategoria(bundle.Products, bundle.Categories);
        BuildTopProdutosMaisVendidos(bundle.Sales, bundle.SaleItems, bundle.Products);
    }

    private void BuildKpis(List<Product> products)
    {
        TotalProdutos = products.Count;
        ProdutosEstoqueZero = products.Count(p => p.StockQuantity == 0);
        ValorEstoqueCusto = products.Sum(p => p.CostPrice * p.StockQuantity);
        ValorEstoqueVenda = products.Sum(p => p.Price * p.StockQuantity);
    }

    private void BuildEstoquePorCategoria(List<Product> products, List<Category> categories)
    {
        var catDict = categories.ToDictionary(c => c.CategoryId, c => c.Name ?? "Sem Categoria");

        var grouped = products
            .GroupBy(p => p.CategoryId)
            .Select(g => new
            {
                Name = g.Key.HasValue && catDict.TryGetValue(g.Key.Value, out var n) ? n : "Sem Categoria",
                Total = (double)g.Sum(p => p.StockQuantity)
            })
            .Where(g => g.Total > 0)
            .ToList();

        EstoquePorCategoriaSeries = grouped.Select(g =>
            (ISeries)new PieSeries<double>
            {
                Name = g.Name,
                Values = new double[] { g.Total },
                InnerRadius = 50,
            }
        ).ToArray();
    }

    private void BuildTopProdutosMaisVendidos(List<Sale> sales, List<SaleItem> saleItems, List<Product> products)
    {
        var now = BrazilDateTime.Now;
        var saleIdsMes = new HashSet<Guid>(
            sales.Where(s =>
                s.Status == SaleStatus.Finalizada &&
                s.SaleDate.Year == now.Year &&
                s.SaleDate.Month == now.Month)
            .Select(s => s.SaleId));

        var prodDict = products.ToDictionary(p => p.ProductId, p => p.Name ?? "?");

        var grouped = saleItems
            .Where(si => saleIdsMes.Contains(si.SaleId))
            .GroupBy(si => si.ProductId)
            .Select(g => new
            {
                Name = prodDict.GetValueOrDefault(g.Key, "?"),
                Qty = g.Sum(si => si.Quantity)
            })
            .OrderByDescending(g => g.Qty)
            .Take(10)
            .ToList();

        var names = grouped.Select(g => g.Name).ToArray();
        var values = grouped.Select(g => (double)g.Qty).ToArray();

        TopProdutosSeries = new ISeries[]
        {
            new RowSeries<double>
            {
                Name = "Quantidade",
                Values = values,
                Fill = new SolidColorPaint(new SKColor(255, 152, 0)),
                MaxBarWidth = 20,
            }
        };

        TopProdutosYAxes = new Axis[]
        {
            new Axis
            {
                Labels = names,
                TextSize = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(220, 220, 220))
            }
        };

        TopProdutosXAxes = new Axis[]
        {
            new Axis
            {
                TextSize = 10,
                LabelsPaint = new SolidColorPaint(new SKColor(220, 220, 220))
            }
        };
    }
}
