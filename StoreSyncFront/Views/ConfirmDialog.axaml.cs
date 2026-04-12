using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace StoreSyncFront.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close(false);
            if (e.Key == Key.Enter) Close(true);
        };
        Opened += (_, _) => Focus();
    }

    private void YesButton_Click(object? sender, RoutedEventArgs e) => Close(true);
    private void NoButton_Click(object? sender, RoutedEventArgs e) => Close(false);
}
