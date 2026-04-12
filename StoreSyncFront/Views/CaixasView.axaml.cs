using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StoreSyncFront.Services;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class CaixasView : UserControl
{
    public CaixasView()
    {
        InitializeComponent();
    }

    private void SearchTextBox_Loaded(object? sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }

    private void SearchTextBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var vm = DataContext as CaixasViewModel;
            vm?.SearchCommand.Execute(null);
        }
    }

    private void CaixasDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && CaixasDataGrid.SelectedItem is SharedModels.Caixa caixa)
        {
            var vm = DataContext as CaixasViewModel;
            if (vm != null)
                _ = vm.AbrirFormulario(caixa.CaixaId);
        }
    }

    private void VerCaixaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid caixaId)
        {
            var vm = DataContext as CaixasViewModel;
            if (vm != null)
                _ = vm.AbrirFormulario(caixaId);
        }
    }

    private async void FecharCaixaButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as CaixasViewModel;
        if (vm == null) return;

        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        vm.IsActionsExpanded = false;

        var dialog = new FecharCaixaDialog();
        var result = await dialog.ShowDialog<decimal?>(window);
        if (result == null) return;

        await vm.FecharCaixa(result.Value);
    }

    private async void AddMovimentacaoButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as CaixasViewModel;
        if (vm == null) return;

        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        vm.IsActionsExpanded = false;

        var dialog = new AddMovimentacaoCaixaDialog();
        var result = await dialog.ShowDialog<(int tipo, string? descricao, decimal valor)?>(window);
        if (result == null) return;

        await vm.AddMovimentacao(result.Value.tipo, result.Value.descricao, result.Value.valor);
    }

    private async void ExportarButton_Click(object? sender, RoutedEventArgs e)
    {
        var vm = DataContext as CaixasViewModel;
        if (vm == null) return;

        var bytes = await vm.DownloadRelatorio();
        if (bytes == null) return;

        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        var dialog = new SaveFileDialog
        {
            Title = "Salvar relatório",
            DefaultExtension = "pdf",
            Filters =
            [
                new FileDialogFilter { Name = "PDF", Extensions = { "pdf" } }
            ],
            InitialFileName = $"Relatorio_Caixa_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
        };

        var path = await dialog.ShowAsync(window);
        if (string.IsNullOrEmpty(path)) return;

        await File.WriteAllBytesAsync(path, bytes);
        SnackBarService.SendSuccess("Relatório exportado com sucesso.");
    }
}
