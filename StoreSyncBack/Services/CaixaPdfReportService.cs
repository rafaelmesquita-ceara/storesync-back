using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SharedModels;

namespace StoreSyncBack.Services;

public class CaixaPdfReportService
{
    public CaixaPdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarRelatorio(Caixa caixa)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, caixa));
                page.Content().Element(c => ComposeContent(c, caixa));
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

    private void ComposeHeader(IContainer container, Caixa caixa)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("StoreSync").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text("Relatório de Caixa").FontSize(14).SemiBold();
                column.Item().Text($"Referência: {caixa.Referencia}").FontSize(11);
                column.Item().Text($"Situação: {caixa.StatusLabel}").FontSize(10).FontColor(Colors.Grey.Darken2);
            });

            row.ConstantItem(130).AlignRight().Text(
                $"Emitido em:\n{DateTime.Now:dd/MM/yyyy HH:mm}"
            ).FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static readonly CultureInfo PtBr = new("pt-BR");

    private void ComposeContent(IContainer container, Caixa caixa)
    {
        container.PaddingVertical(0.5f, Unit.Centimetre).Column(column =>
        {
            column.Spacing(10);

            // Resumo do caixa
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("Abertura");
                    header.Cell().Element(HeaderStyle).Text("Fechamento");
                    header.Cell().Element(HeaderStyle).Text("Valor Abertura");
                    header.Cell().Element(HeaderStyle).Text("Valor Fechamento");

                    static IContainer HeaderStyle(IContainer c) =>
                        c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                table.Cell().Element(CellStyle).Text(caixa.DataAbertura.ToString("dd/MM/yyyy HH:mm"));
                table.Cell().Element(CellStyle).Text(caixa.DataFechamento?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                table.Cell().Element(CellStyle).Text($"R$ {caixa.ValorAbertura.ToString("N2", PtBr)}");
                table.Cell().Element(CellStyle).Text(caixa.ValorFechamento.HasValue ? $"R$ {caixa.ValorFechamento.Value.ToString("N2", PtBr)}" : "-");

                static IContainer CellStyle(IContainer c) =>
                    c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
            });

            // Vendas
            if (caixa.Vendas != null && caixa.Vendas.Count > 0)
            {
                column.Item().Text("Vendas Vinculadas").FontSize(12).SemiBold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("Referência");
                        header.Cell().Element(HeaderStyle).Text("Data");
                        header.Cell().Element(HeaderStyle).Text("Funcionário");
                        header.Cell().Element(HeaderStyle).AlignRight().PaddingRight(8).Text("Total (R$)");
                        header.Cell().Element(HeaderStyle).Text("Situação");

                        static IContainer HeaderStyle(IContainer c) =>
                            c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    foreach (var venda in caixa.Vendas)
                    {
                        table.Cell().Element(CellStyle).Text(venda.Referencia ?? "-");
                        table.Cell().Element(CellStyle).Text(venda.SaleDate.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Element(CellStyle).Text(venda.Employee?.Name ?? "-");
                        table.Cell().Element(CellStyle).AlignRight().PaddingRight(8).Text(venda.TotalAmount.ToString("N2", PtBr));
                        table.Cell().Element(CellStyle).Text(venda.Status == SaleStatus.Finalizada ? "Finalizada" : venda.Status == SaleStatus.Cancelada ? "Cancelada" : "Aberta");

                        static IContainer CellStyle(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
                    }
                });
            }

            // Movimentações
            if (caixa.Movimentacoes != null && caixa.Movimentacoes.Count > 0)
            {
                column.Item().Text("Sangrias e Suprimentos").FontSize(12).SemiBold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("Tipo");
                        header.Cell().Element(HeaderStyle).Text("Descrição");
                        header.Cell().Element(HeaderStyle).AlignRight().PaddingRight(8).Text("Valor (R$)");
                        header.Cell().Element(HeaderStyle).Text("Data");

                        static IContainer HeaderStyle(IContainer c) =>
                            c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    foreach (var mov in caixa.Movimentacoes)
                    {
                        table.Cell().Element(CellStyle).Text(mov.TipoLabel);
                        table.Cell().Element(CellStyle).Text(mov.Descricao ?? "-");
                        table.Cell().Element(CellStyle).AlignRight().PaddingRight(8).Text(mov.Valor.ToString("N2", PtBr));
                        table.Cell().Element(CellStyle).Text(mov.CreatedAt.ToString("dd/MM/yyyy HH:mm"));

                        static IContainer CellStyle(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
                    }
                });
            }

            // Resumo financeiro
            column.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(8).Column(resumo =>
            {
                resumo.Spacing(4);
                resumo.Item().Text("Resumo Financeiro").FontSize(12).SemiBold();

                void Linha(string label, string valor, bool destaque = false)
                {
                    resumo.Item().Row(row =>
                    {
                        var labelStyle = row.RelativeItem().Text(label).FontSize(destaque ? 11 : 10);
                        if (destaque) labelStyle.SemiBold();
                        var valorStyle = row.ConstantItem(120).AlignRight().Text(valor).FontSize(destaque ? 11 : 10);
                        if (destaque) valorStyle.SemiBold();
                    });
                }

                Linha("Valor de Abertura:", $"R$ {caixa.ValorAbertura.ToString("N2", PtBr)}");
                Linha("Total de Vendas (finalizadas):", $"R$ {caixa.TotalVendas.ToString("N2", PtBr)}");
                Linha("Total de Suprimentos:", $"+ R$ {caixa.TotalSuprimentos.ToString("N2", PtBr)}");
                Linha("Total de Sangrias:", $"- R$ {caixa.TotalSangrias.ToString("N2", PtBr)}");
                Linha("Saldo Esperado:", $"R$ {(caixa.ValorAbertura + caixa.TotalVendas + caixa.TotalSuprimentos - caixa.TotalSangrias).ToString("N2", PtBr)}", destaque: true);

                if (caixa.ValorFechamento.HasValue)
                {
                    Linha("Valor de Fechamento (contado):", $"R$ {caixa.ValorFechamento.Value.ToString("N2", PtBr)}");

                    if (caixa.ValorFaltante.HasValue && caixa.ValorFaltante > 0)
                        resumo.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Faltante:").FontSize(11).SemiBold().FontColor(Colors.Red.Darken1);
                            row.ConstantItem(120).AlignRight().Text($"R$ {caixa.ValorFaltante.Value.ToString("N2", PtBr)}").FontSize(11).SemiBold().FontColor(Colors.Red.Darken1);
                        });

                    if (caixa.ValorSobra.HasValue && caixa.ValorSobra > 0)
                        resumo.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Sobra:").FontSize(11).SemiBold().FontColor(Colors.Green.Darken1);
                            row.ConstantItem(120).AlignRight().Text($"R$ {caixa.ValorSobra.Value.ToString("N2", PtBr)}").FontSize(11).SemiBold().FontColor(Colors.Green.Darken1);
                        });
                }
            });
        });
    }
}
