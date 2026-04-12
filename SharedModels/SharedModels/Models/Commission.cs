namespace SharedModels;

public class Commission
{
    public Guid? CommissionId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string? Observation { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CommissionValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
