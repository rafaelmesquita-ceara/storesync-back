namespace SharedModels;

public static class SaleStatus
{
    public const int Aberta = 1;
    public const int Finalizada = 2;
    public const int Cancelada = 3;
}

public class Sale
{
    public Guid SaleId { get; set; }
    public string? Referencia { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public decimal Discount { get; set; }
    public decimal Addition { get; set; }
    public decimal TotalAmount { get; set; }
    public int Status { get; set; } = SaleStatus.Aberta;
    public DateTime SaleDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Troco { get; set; }
    public Guid? CaixaId { get; set; }
    public List<SaleItem>? Items { get; set; }
    public List<SalePayment>? Payments { get; set; }

    public decimal TotalCostSnapshot { get; set; }

    public decimal TotalCost => Items?.Sum(i => i.TotalCost) ?? 0m;
    public decimal GrossProfit => TotalAmount - TotalCost;
    public decimal MarginPercent => TotalAmount == 0 ? 0m : GrossProfit / TotalAmount * 100;

    public decimal GrossProfitSnapshot => TotalAmount - TotalCostSnapshot;
    public decimal MarginPercentSnapshot => TotalAmount == 0 ? 0m : GrossProfitSnapshot / TotalAmount * 100;

    public string StatusLabel => Status switch
    {
        SaleStatus.Aberta => "Aberta",
        SaleStatus.Finalizada => "Finalizada",
        SaleStatus.Cancelada => "Cancelada",
        _ => "Desconhecido"
    };
}