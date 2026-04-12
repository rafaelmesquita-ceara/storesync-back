namespace SharedModels;

public static class CaixaStatus
{
    public const int Aberto = 1;
    public const int Fechado = 2;
}

public class Caixa
{
    public Guid CaixaId { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public decimal ValorAbertura { get; set; }
    public decimal? ValorFechamento { get; set; }
    public decimal TotalVendas { get; set; }
    public decimal TotalSangrias { get; set; }
    public decimal TotalSuprimentos { get; set; }
    public decimal? ValorFaltante { get; set; }
    public decimal? ValorSobra { get; set; }
    public int Status { get; set; } = CaixaStatus.Aberto;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public List<Sale>? Vendas { get; set; }
    public List<MovimentacaoCaixa>? Movimentacoes { get; set; }

    public string StatusLabel => Status == CaixaStatus.Aberto ? "Aberto" : "Fechado";
}
