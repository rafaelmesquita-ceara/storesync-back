using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class ProductsView : UserControl
{
    public ProductsView()
    {
        InitializeComponent();

        this.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    public void RefreshContainerPage_RefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        deferral.Complete();
    }

    private void ProductsDataGrid_KeyUp(object? sender, KeyEventArgs e)
    {
        // Proteções básicas
        if (DataContext is not ViewModels.ProductsViewModel vm) return;
        if (ProductsDataGrid.SelectedItem is not ProductViewModel selected)
            return;

        var id = selected.ProductId;

        // Enter -> Edit
        if (e.Key == Key.F2)
        {
            if (vm.OpenEditCommand is System.Windows.Input.ICommand openCmd && openCmd.CanExecute(id))
                openCmd.Execute(id);

            e.Handled = true;
            return;
        }

        // F4 -> Delete
        if (e.Key == Key.F3)
        {
            if (vm.DeleteCommand is System.Windows.Input.ICommand delCmd && delCmd.CanExecute(id))
                delCmd.Execute(id);

            e.Handled = true;
            return;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F4)
        {
            SearchTextBox.Focus();
            e.Handled = true;
        }
    }

    private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        // Só reage ao Enter
        if (e.Key != Key.Enter) return;

        if (DataContext is not ProductsViewModel vm) return;

        if (vm.SearchCommand != null && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }

        // alternativa direta (se quiser chamar método):
        // vm.Search();
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private async void AddCategoryButton_Click(object? sender, RoutedEventArgs e)
    {
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var dialog = new AddCategoryDialog();
        var name = await dialog.ShowDialog<string?>(parentWindow!);

        if (!string.IsNullOrWhiteSpace(name) && DataContext is ProductsViewModel vm)
            await vm.AddCategoryAsync(name);
    }
}