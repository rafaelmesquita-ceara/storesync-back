using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SharedModels;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class SalesView : UserControl
{
    public SalesView()
    {
        InitializeComponent();
    }

    public void RefreshContainer_RefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        deferral.Complete();
    }

    private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (DataContext is SalesViewModel vm && vm.SearchCommand.CanExecute(null))
        {
            vm.SearchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void SalesDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;
        if (SalesDataGrid.SelectedItem is not SaleViewModel selected) return;

        if (e.Key == Key.F2)
        {
            vm.OpenEditCommand.Execute(selected.SaleId);
            e.Handled = true;
        }
    }

    private async void AdicionarItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;

        var products = await vm.GetProductsAsync();
        var dialog = new AddSaleItemDialog(products);
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var result = await dialog.ShowDialog<(Product product, int qty, decimal discount, decimal addition)?>(parentWindow!);

        if (result.HasValue)
        {
            await vm.AddItemAsync(result.Value.product, result.Value.qty, result.Value.discount, result.Value.addition);
        }
    }

    private async void AdicionarPagamentoButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;

        var methods = await vm.GetPaymentMethodsAsync();
        var dialog = new AddSalePaymentDialog(methods);
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var result = await dialog.ShowDialog<(SharedModels.PaymentMethod method, decimal amount,
            int installments, bool surchargeApplied, decimal surchargeAmount)?>(parentWindow!);

        if (result.HasValue)
        {
            await vm.AddPaymentAsync(
                result.Value.method,
                result.Value.amount,
                result.Value.installments,
                result.Value.surchargeApplied,
                result.Value.surchargeAmount);
        }
    }

    private async void FinalizarVendaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;
        await vm.FinalizeSaleAsync();
        vm.IsActionsExpanded = false;
    }

    private async void CancelarVendaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;
        await vm.CancelSaleAsync();
        vm.IsActionsExpanded = false;
    }

    private async void NovaVendaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null) return;

        // Verifica se há caixa aberto
        var caixa = await vm.GetCaixaAbertoAsync();
        if (caixa == null)
        {
            var confirm = new ConfirmDialog("Não há caixa aberto. Deseja abrir um novo caixa?");
            var confirmar = await confirm.ShowDialog<bool>(parentWindow);
            if (!confirmar) return;

            var abrirDialog = new AbrirCaixaDialog();
            var valorAbertura = await abrirDialog.ShowDialog<decimal?>(parentWindow);
            if (valorAbertura == null) return;

            caixa = await vm.AbrirCaixaAsync(valorAbertura.Value);
            if (caixa == null) return;
        }

        await vm.CriarNovaVendaInternaAsync();
    }

    private async void ExportReportButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SalesViewModel vm) return;

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        var dialog = new ExportReportDialog();
        var result = await dialog.ShowDialog<(System.DateTime start, System.DateTime end)?>(parentWindow!);
        if (result == null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var bytes = await vm.DownloadReportAsync(result.Value.start, result.Value.end);
        if (bytes == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Salvar Relatório de Vendas",
            DefaultExtension = "pdf",
            SuggestedFileName = $"Relatorio_Vendas_{System.DateTime.Now:yyyyMMdd}.pdf",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PDF Document") { Patterns = new[] { "*.pdf" } }
            }
        });

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            await stream.WriteAsync(bytes);
            StoreSyncFront.Services.SnackBarService.SendSuccess("Relatório salvo com sucesso!");
        }
    }
}
