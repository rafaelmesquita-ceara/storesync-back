using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class CommissionsView : UserControl
{
    public CommissionsView()
    {
        InitializeComponent();
    }

    private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is CommissionsViewModel vm && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void CommissionsDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CommissionsViewModel vm) return;
        if (CommissionsDataGrid.SelectedItem is not CommissionViewModel selected) return;

        if (e.Key == Key.F2)
        {
            vm.OpenViewCommand.Execute(selected.CommissionId);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            vm.DeleteCommand.Execute(selected.CommissionId);
            e.Handled = true;
        }
    }

    private async void CalculateButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CommissionsViewModel vm) return;
        await vm.CalculateCommand.ExecuteAsync(null);
    }

    private async void ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CommissionsViewModel vm) return;
        var parentWindow = TopLevel.GetTopLevel(this) as Window;

        // Pergunta se deseja lançar conta a pagar
        var createFinance = await ShowConfirm(
            parentWindow,
            "Deseja lançar uma conta a pagar referente a este comissionamento?");

        Guid? financeId = null;

        if (createFinance)
        {
            financeId = await vm.CreateFinanceForCommissionAsync();

            if (financeId.HasValue)
            {
                // Pergunta se deseja dar baixa imediatamente
                var settleNow = await ShowConfirm(
                    parentWindow,
                    "Conta a pagar criada com sucesso. Deseja dar baixa nessa conta agora?");

                if (settleNow)
                    await vm.SettleFinanceAsync(financeId.Value);
            }
        }

        // Salva a comissão independentemente da decisão financeira
        await vm.ConfirmCreateAsync();
    }

    private static async Task<bool> ShowConfirm(Window? parent, string message)
    {
        if (parent == null) return false;
        var dialog = new ConfirmDialog(message);
        var result = await dialog.ShowDialog<bool?>(parent);
        return result == true;
    }
}
