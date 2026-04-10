using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;

namespace StoreSyncFront.Views;

public partial class ProductSearchDialog : Window
{
    private List<Product> _allProducts = new();

    public ProductSearchDialog()
    {
        InitializeComponent();
        Opened += (_, _) => SearchBox.Focus();
    }

    public void SetProducts(IEnumerable<Product> products)
    {
        _allProducts = products.ToList();
        ProductsGrid.ItemsSource = _allProducts;
    }

    private void SearchBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ApplyFilter();
        if (e.Key == Key.Escape)
            Close(null);
    }

    private void SearchButton_Click(object? sender, RoutedEventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = (SearchBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            ProductsGrid.ItemsSource = _allProducts;
            return;
        }

        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Select(Normalize)
                          .ToArray();

        var filtered = _allProducts.Where(p =>
        {
            var combined = new StringBuilder();
            combined.Append(p.Reference ?? string.Empty).Append(' ');
            combined.Append(p.Name ?? string.Empty).Append(' ');
            combined.Append(p.Category?.Name ?? string.Empty);
            var norm = Normalize(combined.ToString());
            return tokens.All(t => norm.Contains(t));
        }).ToList();

        ProductsGrid.ItemsSource = filtered;
    }

    private void ProductsGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        TryConfirm();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void TryConfirm()
    {
        if (ProductsGrid.SelectedItem is Product product)
            Close(product);
    }

    private static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
