namespace SharedModels;

public static class MovimentacaoTipo
{
    public const int Sangria = 1;
    public const int Suprimento = 2;
}

public class MovimentacaoCaixa
{
    public Guid MovimentacaoCaixaId { get; set; }
    public Guid CaixaId { get; set; }
    public int Tipo { get; set; }
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public DateTime CreatedAt { get; set; }

    public string TipoLabel => Tipo == MovimentacaoTipo.Sangria ? "Sangria" : "Suprimento";
}
