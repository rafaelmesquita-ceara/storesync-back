namespace SharedModels;
public class Employee
{
    public Guid EmployeeId { get; set; }
    public string? Name { get; set; }
    public string? Cpf { get; set; }
    public string? Role { get; set; }
    public decimal CommissionRate { get; set; }
    public DateTime CreatedAt { get; set; }
}