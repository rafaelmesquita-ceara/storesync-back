using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace StoreSyncFront.Views;

public partial class AddCategoryDialog : Window
{
    public AddCategoryDialog()
    {
        InitializeComponent();
        Opened += (_, _) => CategoryNameBox.Focus();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void CategoryNameBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
    }

    private void TryConfirm()
    {
        var name = CategoryNameBox.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
            Close(name);
    }
}
