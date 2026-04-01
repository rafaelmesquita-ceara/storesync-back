namespace SharedModels;

public class Sale
{
    public Guid SaleId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public required List<SaleItem>? Items { get; set; }
}