using Avalonia.Controls;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Views;

public partial class PaymentMethodsView : UserControl
{
    public PaymentMethodsView()
    {
        InitializeComponent();
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PaymentMethodsViewModel vm) return;
        if (sender is Button btn && btn.Tag is PaymentMethod pm)
            vm.OpenEdit(pm);
    }
}
