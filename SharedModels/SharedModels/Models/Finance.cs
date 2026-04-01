namespace SharedModels;

public class Finance
{
    public Guid FinanceId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}