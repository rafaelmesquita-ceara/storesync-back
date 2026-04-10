using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class CategoriesView : UserControl
{
    public CategoriesView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnViewKeyDown, RoutingStrategies.Tunnel);
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is not CategoriesViewModel vm) return;
        if (vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void NewCategoryBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is not CategoriesViewModel vm) return;
        if (vm.AddCategoryCommand.CanExecute(null))
        {
            vm.AddCategoryCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control c || c.Tag is not System.Guid categoryId) return;
        if (DataContext is not CategoriesViewModel vm) return;
        vm.BeginEdit(categoryId);
    }

    private async void InlineEditBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (tb.DataContext is not CategoryRowViewModel row) return;
        if (DataContext is not CategoriesViewModel vm) return;

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await vm.CommitEdit(row);
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            vm.CancelEdit(row);
        }
    }

    private async void InlineEditBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (tb.DataContext is not CategoryRowViewModel row || !row.IsEditing) return;
        if (DataContext is not CategoriesViewModel vm) return;
        await vm.CommitEdit(row);
    }

    private void CategoriesDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CategoriesViewModel vm) return;
        if (CategoriesDataGrid.SelectedItem is not CategoryRowViewModel selected) return;

        if (e.Key == Key.F2)
        {
            vm.BeginEdit(selected.CategoryId);
            e.Handled = true;
        }
        else if (e.Key == Key.F3)
        {
            if (vm.DeleteCommand.CanExecute(selected.CategoryId))
                vm.DeleteCommand.Execute(selected.CategoryId);
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
}
