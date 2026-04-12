using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.Services;

namespace StoreSyncFront.Views;

public partial class AddSaleItemDialog : Window
{
    private Product? _selectedProduct;
    private readonly IEnumerable<Product> _products;

    public AddSaleItemDialog(IEnumerable<Product> products)
    {
        _products = products;
        InitializeComponent();
        Opened += (_, _) => QuantityBox.Focus();
    }

    private async void SearchProductButton_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new ProductSearchDialog();
        dialog.SetProducts(_products);
        var parentWindow = TopLevel.GetTopLevel(this) as Window ?? this;
        var result = await dialog.ShowDialog<Product?>(parentWindow);
        if (result != null)
        {
            _selectedProduct = result;
            ProductBox.Text = $"{result.Reference} - {result.Name}";
            StockBox.Text = result.StockQuantity.ToString();
            UnitPriceBox.Text = result.Price.ToString("N2", CultureInfo.CurrentCulture);
            RecalculateTotal();
        }
    }

    private void Box_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
        if (e.Key == Key.Escape) Close(null);
        if (e.Key == Key.Tab) RecalculateTotal();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void RecalculateTotal()
    {
        if (_selectedProduct == null) return;

        int.TryParse(QuantityBox.Text, out int qty);
        decimal.TryParse((DiscountBox.Text ?? "0").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal discount);
        decimal.TryParse((AdditionBox.Text ?? "0").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal addition);

        var total = (qty * _selectedProduct.Price) - discount + addition;
        TotalBox.Text = total.ToString("N2", CultureInfo.CurrentCulture);
    }

    private void TryConfirm()
    {
        RecalculateTotal();

        if (_selectedProduct == null)
        {
            SnackBarService.SendWarning("Selecione um produto.");
            return;
        }

        if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
        {
            SnackBarService.SendWarning("Informe uma quantidade válida maior que zero.");
            return;
        }

        if (qty > _selectedProduct.StockQuantity)
        {
            SnackBarService.SendWarning($"Estoque insuficiente. Disponível: {_selectedProduct.StockQuantity}.");
            return;
        }

        decimal.TryParse((DiscountBox.Text ?? "0").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal discount);
        decimal.TryParse((AdditionBox.Text ?? "0").Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal addition);

        var result = (_selectedProduct, qty, discount, addition);
        Close(result);
    }
}
