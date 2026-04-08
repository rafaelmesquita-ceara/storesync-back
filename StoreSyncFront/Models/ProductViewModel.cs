using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

public partial class ProductViewModel : ObservableObject
{
    public Product Model { get; }

    [ObservableProperty] private Guid productId;
    [ObservableProperty] private string reference;
    [ObservableProperty] private string name;
    [ObservableProperty] private int stockQuantity;
    [ObservableProperty] private decimal price;
    [ObservableProperty] private Category category;
    [ObservableProperty] private DateTime createdAt;

    public ProductViewModel(Product product)
    {
        Model = product;

        ProductId = product.ProductId;
        Reference = product.Reference;
        Name = product.Name;
        StockQuantity = product.StockQuantity;
        Price = product.Price;
        Category = product.Category;
        CreatedAt = product.CreatedAt;
    }

    public void ApplyChangesToModel()
    {
        Model.Reference = Reference;
        Model.Name = Name;
        Model.StockQuantity = StockQuantity;
        Model.Price = Price;
        Model.Category = Category;
        Model.CreatedAt = CreatedAt;
    }
}