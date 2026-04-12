using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class ClientsView : UserControl
{
    public ClientsView()
    {
        InitializeComponent();

        this.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void ClientsDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ClientsViewModel vm) return;
        if (ClientsDataGrid.SelectedItem is not Client selected) return;

        var id = selected.ClientId;

        if (e.Key == Key.F2)
        {
            if (vm.OpenEditCommand is System.Windows.Input.ICommand openCmd && openCmd.CanExecute(id))
                openCmd.Execute(id);

            e.Handled = true;
            return;
        }

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
        if (e.Key != Key.Enter) return;

        if (DataContext is not ClientsViewModel vm) return;

        if (vm.SearchCommand != null && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }
}
