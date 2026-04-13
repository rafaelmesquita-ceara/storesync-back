namespace SharedModels;

public class Product 
{
    public Guid ProductId { get; set; }
    public string? Reference { get; set; }
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}