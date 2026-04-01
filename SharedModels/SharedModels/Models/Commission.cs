namespace SharedModels;

public class Commission
{
    public Guid? CommissionId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime Month { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CommissionValue { get; set; }
    public DateTime CreatedAt { get; set; }
}