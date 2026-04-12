using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SharedModels;

namespace StoreSyncBack.Services;

public class SalesPdfReportService
{
    public SalesPdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateSalesReport(IEnumerable<Sale> sales, DateTime startDate, DateTime endDate)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, startDate, endDate));
                page.Content().Element(c => ComposeContent(c, sales));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, DateTime start, DateTime end)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("StoreSync").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Relatório Semanal de Vendas Finalizadas").FontSize(14).SemiBold();
                column.Item().Text($"Período Analisado: {start:dd/MM/yyyy} a {end:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(100).AlignRight().Text($"Emitido em:\n{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private void ComposeContent(IContainer container, IEnumerable<Sale> sales)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.5f); // Data
                    columns.RelativeColumn(2); // Referencia
                    columns.RelativeColumn(3); // Funcionario
                    columns.RelativeColumn(2); // Total
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Data do Registo");
                    header.Cell().Element(CellStyle).Text("Referência / Cód Venda");
                    header.Cell().Element(CellStyle).Text("Funcionário Titular");
                    header.Cell().Element(CellStyle).AlignRight().Text("Valor Total faturado (R$)");

                    static IContainer CellStyle(IContainer c)
                    {
                        return c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                foreach (var sale in sales)
                {
                    table.Cell().Element(CellStyle).Text(sale.SaleDate.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellStyle).Text(sale.Referencia);
                    table.Cell().Element(CellStyle).Text(sale.Employee?.Name ?? "N/A");
                    table.Cell().Element(CellStyle).AlignRight().Text(sale.TotalAmount.ToString("N2"));

                    static IContainer CellStyle(IContainer c)
                    {
                        return c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
                    }
                }
            });

            var totalSum = sales.Sum(s => s.TotalAmount);
            column.Item().PaddingTop(10).AlignRight().Text($"Total do Período: R$ {totalSum:N2}").FontSize(14).SemiBold();
        });
    }
}
