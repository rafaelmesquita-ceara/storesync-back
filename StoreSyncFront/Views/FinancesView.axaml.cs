using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class FinancesView : UserControl
{
    public FinancesView()
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
        if (DataContext is FinancesViewModel vm && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void FinancesDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not FinancesViewModel vm) return;
        if (FinancesDataGrid.SelectedItem is not FinanceViewModel selected) return;

        if (e.Key == Key.F2)
        {
            vm.OpenEditCommand.Execute(selected.FinanceId);
            e.Handled = true;
        }
        else if (e.Key == Key.F3)
        {
            vm.DeleteCommand.Execute(selected.FinanceId);
            e.Handled = true;
        }
    }

    private async void LiquidarButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not FinancesViewModel vm) return;
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var dialog = new SettleDialog();
        var result = await dialog.ShowDialog<(decimal amount, string? note)?>(parentWindow!);
        if (result.HasValue)
            await vm.ConfirmSettleAsync(result.Value.amount, result.Value.note);

        vm.IsActionsExpanded = false;
    }

    private async void CancelarLiquidacaoButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not FinancesViewModel vm) return;
        await vm.ConfirmCancelSettlementAsync();
        vm.IsActionsExpanded = false;
    }
}
