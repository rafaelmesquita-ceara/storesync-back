using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace StoreSyncFront.Views;

public partial class AddRoleDialog : Window
{
    public AddRoleDialog()
    {
        InitializeComponent();
        Opened += (_, _) => RoleNameBox.Focus();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void RoleNameBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
    }

    private void TryConfirm()
    {
        var name = RoleNameBox.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
            Close(name);
    }
}
