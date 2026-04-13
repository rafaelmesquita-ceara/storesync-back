using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SharedModels;
using SkiaSharp;

namespace StoreSyncFront.ViewModels.Dashboard;

public partial class VisaoGeralDashboardViewModel : DashboardPageViewModelBase
{
    // KPIs
    [ObservableProperty] private int _totalVendasMes;
    [ObservableProperty] private decimal _faturamentoMes;
    [ObservableProperty] private decimal _aReceberAberto;
    [ObservableProperty] private decimal _aPagarAberto;

    // Gráfico — Vendas por Dia (Linha)
    [ObservableProperty] private ISeries[] _vendasPorDiaSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _vendasPorDiaXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _vendasPorDiaYAxes = Array.Empty<Axis>();

    // Gráfico — Lucro Bruto por Dia (Linha)
    [ObservableProperty] private ISeries[] _lucroBrutoPorDiaSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _lucroBrutoPorDiaXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _lucroBrutoPorDiaYAxes = Array.Empty<Axis>();

    public VisaoGeralDashboardViewModel()
    {
        Title = "Visão Geral";
    }

    public override void BuildFromData(DashboardDataBundle bundle)
    {
        BuildKpis(bundle.Sales, bundle.Finances);
        BuildVendasPorDia(bundle.Sales);
        BuildLucroBrutoPorDia(bundle.Sales);
    }

    private void BuildKpis(List<Sale> sales, List<Finance> finances)
    {
        var now = BrazilDateTime.Now;
        var vendasMes = sales.Where(s =>
            s.Status == SaleStatus.Finalizada &&
            s.SaleDate.Year == now.Year &&
            s.SaleDate.Month == now.Month).ToList();

        TotalVendasMes = vendasMes.Count;
        FaturamentoMes = vendasMes.Sum(s => s.TotalAmount);
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
            new Axis { Labels = labels, LabelsRotation = 45, TextSize = 10 }
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

    private void BuildLucroBrutoPorDia(List<Sale> sales)
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
                .Sum(s => s.GrossProfitSnapshot);
        }

        LucroBrutoPorDiaSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Lucro Bruto (R$)",
                Values = values,
                Fill = null,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(76, 175, 80)) { StrokeThickness = 2 },
                GeometryStroke = new SolidColorPaint(new SKColor(76, 175, 80)),
                GeometryFill = new SolidColorPaint(new SKColor(76, 175, 80)),
            }
        };

        LucroBrutoPorDiaXAxes = new Axis[]
        {
            new Axis { Labels = labels, LabelsRotation = 45, TextSize = 10 }
        };

        LucroBrutoPorDiaYAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => value.ToString("C0", CultureInfo.GetCultureInfo("pt-BR")),
                TextSize = 10,
            }
        };
    }
}
