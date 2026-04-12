using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.Services;

namespace StoreSyncFront.Views;

public partial class AddMovimentacaoCaixaDialog : Window
{
    public AddMovimentacaoCaixaDialog()
    {
        InitializeComponent();

        TipoCombo.ItemsSource = new[] { "Sangria", "Suprimento" };
        TipoCombo.SelectedIndex = 0;

        Opened += (_, _) => ValorBox.Focus();
    }

    private void ConfirmarButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();

    private void CancelarButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void ValorBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
        if (e.Key == Key.Escape) Close(null);
    }

    private void TryConfirm()
    {
        var raw = (ValorBox.Text ?? string.Empty).Replace(',', '.');
        if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor) || valor <= 0)
        {
            SnackBarService.SendWarning("Informe um valor válido maior que zero.");
            return;
        }

        var tipo = TipoCombo.SelectedIndex == 0 ? MovimentacaoTipo.Sangria : MovimentacaoTipo.Suprimento;
        var descricao = DescricaoBox.Text?.Trim();

        Close((tipo, descricao, valor));
    }
}
