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
    public decimal Discount { get; set; }
    public decimal Addition { get; set; }
    public decimal TotalAmount { get; set; }
    public int Status { get; set; } = SaleStatus.Aberta;
    public DateTime SaleDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SaleItem>? Items { get; set; }
}