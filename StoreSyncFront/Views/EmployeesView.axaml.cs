using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();

        this.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void EmployeesDataGrid_KeyUp(object? sender, KeyEventArgs e)
    {
        if (DataContext is not EmployeesViewModel vm) return;
        if (EmployeesDataGrid.SelectedItem is not Employee selected) return;

        var id = selected.EmployeeId;

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

        if (DataContext is not EmployeesViewModel vm) return;

        if (vm.SearchCommand != null && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private async void AddRoleButton_Click(object? sender, RoutedEventArgs e)
    {
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var dialog = new AddRoleDialog();
        var role = await dialog.ShowDialog<string?>(parentWindow!);

        if (!string.IsNullOrWhiteSpace(role) && DataContext is EmployeesViewModel vm)
            vm.AddRole(role);
    }
}
