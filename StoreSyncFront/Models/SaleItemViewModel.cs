using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

public partial class SaleItemViewModel : ObservableObject
{
    public SaleItem Model { get; }

    [ObservableProperty] private Guid saleItemId;
    [ObservableProperty] private Guid saleId;
    [ObservableProperty] private Guid productId;
    [ObservableProperty] private string? productReference;
    [ObservableProperty] private string? productName;
    [ObservableProperty] private decimal unitPrice;
    [ObservableProperty] private int quantity;
    [ObservableProperty] private decimal discount;
    [ObservableProperty] private decimal addition;
    [ObservableProperty] private decimal totalPrice;
    [ObservableProperty] private int stockQuantity;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private decimal totalCost;
    [ObservableProperty] private decimal grossProfit;
    [ObservableProperty] private decimal marginPercent;

    public SaleItemViewModel(SaleItem item)
    {
        Model = item;

        SaleItemId = item.SaleItemId;
        SaleId = item.SaleId;
        ProductId = item.ProductId;
        ProductReference = item.Product?.Reference;
        ProductName = item.Product?.Name;
        UnitPrice = item.Product?.Price ?? 0;
        Quantity = item.Quantity;
        Discount = item.Discount;
        Addition = item.Addition;
        TotalPrice = item.TotalPrice;
        StockQuantity = item.Product?.StockQuantity ?? 0;
        CostPrice = item.CostPrice;
        TotalCost = item.TotalCost;
        GrossProfit = item.GrossProfit;
        MarginPercent = item.MarginPercent;
    }
}
