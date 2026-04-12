using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.Services;

namespace StoreSyncFront.Views;

public partial class SettleDialog : Window
{
    public SettleDialog()
    {
        InitializeComponent();
        Opened += (_, _) => AmountBox.Focus();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void Box_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
        if (e.Key == Key.Escape) Close(null);
    }

    private void TryConfirm()
    {
        var raw = (AmountBox.Text ?? string.Empty).Replace(',', '.');
        if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
        {
            SnackBarService.SendWarning("Informe um valor válido maior que zero.");
            return;
        }

        var note = NoteBox.Text?.Trim();
        Close((amount, note));
    }
}
