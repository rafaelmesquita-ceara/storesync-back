using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StoreSyncFront.Views;

public partial class ExportReportDialogViewModel : ObservableObject
{
    [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today;
}

public partial class ExportReportDialog : Window
{
    private readonly ExportReportDialogViewModel _vm = new();

    public ExportReportDialog()
    {
        InitializeComponent();
        DataContext = _vm;
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close(null);
        };
        Opened += (_, _) => Focus();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void GenerateButton_Click(object? sender, RoutedEventArgs e)
    {
        var start = _vm.StartDate ?? DateTime.Today.AddDays(-7);
        var end = _vm.EndDate ?? DateTime.Today;
        Close((start, end));
    }
}
