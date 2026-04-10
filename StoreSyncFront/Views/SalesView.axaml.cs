using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
    }

    public void RefreshContainer_RefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        deferral.Complete();
    }

    private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is SalesViewModel vm && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void SalesDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;
        if (SalesDataGrid.SelectedItem is not SaleViewModel selected) return;

        if (e.Key == Key.F2)
        {
            vm.OpenEditCommand.Execute(selected.SaleId);
            e.Handled = true;
        }
    }

    private async void AdicionarItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;

        var products = await vm.GetProductsAsync();
        var dialog = new AddSaleItemDialog(products);
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var result = await dialog.ShowDialog<(Product product, int qty, decimal discount, decimal addition)?>(parentWindow!);

        if (result.HasValue)
        {
            await vm.AddItemAsync(result.Value.product, result.Value.qty, result.Value.discount, result.Value.addition);
        }
    }

    private async void FinalizarVendaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;
        await vm.FinalizeSaleAsync();
        vm.IsActionsExpanded = false;
    }

    private async void CancelarVendaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;
        await vm.CancelSaleAsync();
        vm.IsActionsExpanded = false;
    }
}
