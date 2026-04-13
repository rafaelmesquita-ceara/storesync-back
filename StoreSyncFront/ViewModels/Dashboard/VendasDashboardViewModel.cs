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

public partial class VendasDashboardViewModel : DashboardPageViewModelBase
{
    // KPIs
    [ObservableProperty] private decimal _ticketMedio;
    [ObservableProperty] private decimal _margemMedia;
    [ObservableProperty] private int _totalItensVendidosMes;
    [ObservableProperty] private int _vendasCanceladasMes;

    // Gráfico — Vendas por Funcionário (Barras horizontais)
    [ObservableProperty] private ISeries[] _vendasPorFuncionarioSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] _vendasPorFuncionarioXAxes = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] _vendasPorFuncionarioYAxes = Array.Empty<Axis>();

    // Gráfico — Vendas por Forma de Pagamento (Rosca)
    [ObservableProperty] private IEnumerable<ISeries> _vendasPorFormaPagamentoSeries = Array.Empty<ISeries>();

    public VendasDashboardViewModel()
    {
        Title = "Vendas";
    }

    public override void BuildFromData(DashboardDataBundle bundle)
    {
        BuildKpis(bundle.Sales, bundle.SaleItems);
        BuildVendasPorFuncionario(bundle.Sales, bundle.Employees);
        BuildVendasPorFormaPagamento(bundle.SalePayments, bundle.PaymentMethods);
    }

    private void BuildKpis(List<Sale> sales, List<SaleItem> saleItems)
    {
        var now = BrazilDateTime.Now;
        var vendasMes = sales.Where(s =>
            s.Status == SaleStatus.Finalizada &&
            s.SaleDate.Year == now.Year &&
            s.SaleDate.Month == now.Month).ToList();

        var count = vendasMes.Count;
        var faturamento = vendasMes.Sum(s => s.TotalAmount);

        TicketMedio = count > 0 ? faturamento / count : 0;
        MargemMedia = count > 0 ? vendasMes.Average(s => s.MarginPercentSnapshot) : 0;

        var saleIdsMes = new HashSet<Guid>(vendasMes.Select(s => s.SaleId));
        TotalItensVendidosMes = saleItems
            .Where(si => saleIdsMes.Contains(si.SaleId))
            .Sum(si => si.Quantity);

        VendasCanceladasMes = sales.Count(s =>
            s.Status == SaleStatus.Cancelada &&
            s.SaleDate.Year == now.Year &&
            s.SaleDate.Month == now.Month);
    }

    private void BuildVendasPorFuncionario(List<Sale> sales, List<Employee> employees)
    {
        var empDict = employees.ToDictionary(e => e.EmployeeId, e => e.Name ?? "?");

        var grouped = sales
            .Where(s => s.Status == SaleStatus.Finalizada)
            .GroupBy(s => s.EmployeeId)
            .Select(g => new
            {
                Name = empDict.GetValueOrDefault(g.Key, "?"),
                Total = (double)g.Sum(s => s.TotalAmount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var names = grouped.Select(g => g.Name).ToArray();
        var values = grouped.Select(g => g.Total).ToArray();

        VendasPorFuncionarioSeries = new ISeries[]
        {
            new RowSeries<double>
            {
                Name = "Total (R$)",
                Values = values,
                Fill = new SolidColorPaint(new SKColor(33, 150, 243)),
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

    private void BuildVendasPorFormaPagamento(List<SalePayment> salePayments, List<PaymentMethod> paymentMethods)
    {
        var pmDict = paymentMethods.ToDictionary(p => p.PaymentMethodId, p => p.Name ?? "?");

        var grouped = salePayments
            .GroupBy(sp => sp.PaymentMethodId)
            .Select(g => new
            {
                Name = pmDict.GetValueOrDefault(g.Key, g.First().PaymentMethod?.Name ?? "?"),
                Total = (double)g.Sum(sp => sp.Amount)
            })
            .Where(g => g.Total > 0)
            .OrderByDescending(g => g.Total)
            .ToList();

        VendasPorFormaPagamentoSeries = grouped.Select(g =>
            (ISeries)new PieSeries<double>
            {
                Name = g.Name,
                Values = new double[] { g.Total },
                InnerRadius = 50,
            }
        ).ToArray();
    }
}
