namespace SharedModels;

public static class FinanceStatus
{
    public const int Aberto = 1;
    public const int Liquidado = 2;
    public const int LiquidadoParcialmente = 3;
}

public static class FinanceType
{
    public const int Pagar = 1;
    public const int Receber = 2;
}

public static class FinanceTitleType
{
    public const int Original = 1;
    public const int Residual = 2;
}

public class Finance
{
    public Guid FinanceId { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public int Status { get; set; } = FinanceStatus.Aberto;
    public int Type { get; set; } = FinanceType.Pagar;
    public int TitleType { get; set; } = FinanceTitleType.Original;
    public decimal? SettledAmount { get; set; }
    public DateTime? SettledAt { get; set; }
    public string? SettledNote { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
}
