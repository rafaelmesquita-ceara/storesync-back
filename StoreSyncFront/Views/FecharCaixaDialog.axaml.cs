using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.Services;

namespace StoreSyncFront.Views;

public partial class FecharCaixaDialog : Window
{
    public FecharCaixaDialog()
    {
        InitializeComponent();
        Opened += (_, _) => ValorBox.Focus();
    }

    private void FecharButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void CancelarButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void ValorBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
        if (e.Key == Key.Escape) Close(null);
    }

    private void TryConfirm()
    {
        var raw = (ValorBox.Text ?? string.Empty).Replace(',', '.');
        if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor) || valor < 0)
        {
            SnackBarService.SendWarning("Informe um valor de fechamento válido.");
            return;
        }
        Close(valor);
    }
}
