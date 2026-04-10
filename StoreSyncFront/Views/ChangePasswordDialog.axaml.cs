using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.Services;

namespace StoreSyncFront.Views;

public partial class ChangePasswordDialog : Window
{
    public ChangePasswordDialog()
    {
        InitializeComponent();
        Opened += (_, _) => NewPasswordBox.Focus();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void PasswordBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
    }

    private void TryConfirm()
    {
        var nova = NewPasswordBox.Text ?? string.Empty;
        var confirma = ConfirmPasswordBox.Text ?? string.Empty;

        if (nova.Length < 6)
        {
            SnackBarService.Send("A senha deve ter pelo menos 6 caracteres.");
            return;
        }

        if (nova != confirma)
        {
            SnackBarService.Send("A senha e a confirmação não coincidem.");
            return;
        }

        Close(nova);
    }
}
