using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class UsersView : UserControl
{
    public UsersView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnViewKeyDown, RoutingStrategies.Tunnel);
    }

    private void UsersDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not UsersViewModel vm) return;
        if (UsersDataGrid.SelectedItem is not UserRowViewModel selected) return;

        if (e.Key == Key.F2)
        {
            vm.BeginLoginEdit(selected.UserId);
            e.Handled = true;
        }
        else if (e.Key == Key.F3)
        {
            if (vm.DeleteCommand.CanExecute(selected.UserId))
                vm.DeleteCommand.Execute(selected.UserId);
            e.Handled = true;
        }
    }

    private void OnViewKeyDown(object? sender, KeyEventArgs e)
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
        if (DataContext is not UsersViewModel vm) return;
        if (vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void LoginEditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control c || c.Tag is not Guid userId) return;
        if (DataContext is not UsersViewModel vm) return;
        vm.BeginLoginEdit(userId);
    }

    private async void InlineLoginBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (tb.DataContext is not UserRowViewModel row) return;
        if (DataContext is not UsersViewModel vm) return;

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await vm.CommitLoginEdit(row);
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            vm.CancelLoginEdit(row);
        }
    }

    private async void InlineLoginBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (tb.DataContext is not UserRowViewModel row || !row.IsEditing) return;
        if (DataContext is not UsersViewModel vm) return;
        await vm.CommitLoginEdit(row);
    }

    private async void ChangePasswordButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control c || c.Tag is not Guid userId) return;
        if (DataContext is not UsersViewModel vm) return;

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var dialog = new ChangePasswordDialog();
        var newPassword = await dialog.ShowDialog<string?>(parentWindow!);

        if (!string.IsNullOrEmpty(newPassword))
            await vm.ChangePasswordAsync(userId, newPassword);
    }
}
