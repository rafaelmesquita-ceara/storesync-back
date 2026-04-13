namespace SharedModels;

public class SaleItem
{
    public Guid SaleItemId { get; set; }
    public Guid SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal Addition { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal CostPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public decimal TotalCost => Quantity * CostPrice;
    public decimal GrossProfit => TotalPrice - TotalCost;
    public decimal MarginPercent => TotalPrice == 0 ? 0m : GrossProfit / TotalPrice * 100;
}