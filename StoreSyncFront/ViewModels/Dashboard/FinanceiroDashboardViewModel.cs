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

public partial class FinanceiroDashboardViewModel : DashboardPageViewModelBase
{
    // KPIs
    [ObservableProperty] private int _contasVencidasPagar;
    [ObservableProperty] private int _contasVencidasReceber;
    [ObservableProperty] private decimal _totalLiquidadoMesPagar;
    [ObservableProperty] private decimal _totalLiquidadoMesReceber;

    // Gráfico — A Pagar vs A Receber (Barras agrupadas)
    [ObservableProperty] private ISeries[] _financeiroPagarReceberSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _financeiroPagarReceberXAxes = Array.Empty<Axis>();

    // Gráfico — Status das Contas (Rosca)
    [ObservableProperty] private IEnumerable<ISeries> _statusContasSeries = Array.Empty<ISeries>();

    public FinanceiroDashboardViewModel()
    {
        Title = "Financeiro";
    }

    public override void BuildFromData(DashboardDataBundle bundle)
    {
        BuildKpis(bundle.Finances);
        BuildFinanceiroPagarReceber(bundle.Finances);
        BuildStatusContas(bundle.Finances);
    }

    private void BuildKpis(List<Finance> finances)
    {
        var now = BrazilDateTime.Now;
        var today = now.Date;

        ContasVencidasPagar = finances.Count(f =>
            f.Type == FinanceType.Pagar &&
            f.Status == FinanceStatus.Aberto &&
            f.DueDate.Date < today);

        ContasVencidasReceber = finances.Count(f =>
            f.Type == FinanceType.Receber &&
            f.Status == FinanceStatus.Aberto &&
            f.DueDate.Date < today);

        TotalLiquidadoMesPagar = finances
            .Where(f => f.Type == FinanceType.Pagar &&
                        f.SettledAt.HasValue &&
                        f.SettledAt.Value.Year == now.Year &&
                        f.SettledAt.Value.Month == now.Month)
            .Sum(f => f.SettledAmount ?? 0);

        TotalLiquidadoMesReceber = finances
            .Where(f => f.Type == FinanceType.Receber &&
                        f.SettledAt.HasValue &&
                        f.SettledAt.Value.Year == now.Year &&
                        f.SettledAt.Value.Month == now.Month)
            .Sum(f => f.SettledAmount ?? 0);
    }

    private void BuildFinanceiroPagarReceber(List<Finance> finances)
    {
        var statusLabels = new[] { "Aberto", "Liquidado", "Liq. Parcial" };
        var pagarValues = new double[3];
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
            if (f.Type == FinanceType.Pagar) pagarValues[idx] += (double)f.Amount;
            if (f.Type == FinanceType.Receber) receberValues[idx] += (double)f.Amount;
        }

        FinanceiroPagarReceberSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "A Pagar",
                Values = pagarValues,
                Fill = new SolidColorPaint(new SKColor(244, 67, 54)),
                MaxBarWidth = 40,
            },
            new ColumnSeries<double>
            {
                Name = "A Receber",
                Values = receberValues,
                Fill = new SolidColorPaint(new SKColor(76, 175, 80)),
                MaxBarWidth = 40,
            }
        };

        FinanceiroPagarReceberXAxes = new Axis[]
        {
            new Axis { Labels = statusLabels, TextSize = 11 }
        };
    }

    private void BuildStatusContas(List<Finance> finances)
    {
        var statusLabels = new Dictionary<int, string>
        {
            { FinanceStatus.Aberto, "Aberto" },
            { FinanceStatus.Liquidado, "Liquidado" },
            { FinanceStatus.LiquidadoParcialmente, "Liq. Parcial" }
        };

        StatusContasSeries = statusLabels
            .Select(kv => (ISeries)new PieSeries<double>
            {
                Name = kv.Value,
                Values = new double[] { finances.Count(f => f.Status == kv.Key) },
                InnerRadius = 60,
            })
            .ToArray();
    }
}
