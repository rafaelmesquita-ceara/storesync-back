using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedModels;
using StoreSyncFront.Services;

namespace StoreSyncFront.Views;

public class RateDisplayItem
{
    public PaymentMethodRate Rate { get; }
    public string InstallmentsLabel { get; }

    public RateDisplayItem(PaymentMethodRate rate)
    {
        Rate = rate;
        InstallmentsLabel = $"{rate.Installments}x - {rate.RatePercentage:0.##}%";
    }
}

public partial class AddSalePaymentDialog : Window
{
    private readonly IEnumerable<PaymentMethod> _paymentMethods;
    private PaymentMethod? _selectedMethod;
    private PaymentMethodRate? _selectedRate;

    public AddSalePaymentDialog(IEnumerable<PaymentMethod> paymentMethods)
    {
        _paymentMethods = paymentMethods.Where(pm => pm.Status == PaymentMethodStatus.Ativo).ToList();
        InitializeComponent();

        PaymentMethodCombo.ItemsSource = _paymentMethods;
        Opened += (_, _) => AmountBox.Focus();
    }

    private void PaymentMethodCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedMethod = PaymentMethodCombo.SelectedItem as PaymentMethod;
        _selectedRate = null;

        if (_selectedMethod == null)
        {
            RatesCombo.IsVisible = false;
            SurchargeCheck.IsVisible = false;
            SurchargeBox.IsVisible = false;
            RecalcAll();
            return;
        }

        bool isCard = _selectedMethod.Type == PaymentMethodType.DebitCard ||
                      _selectedMethod.Type == PaymentMethodType.CreditCard;

        if (isCard)
        {
            var items = (_selectedMethod.Rates ?? new List<PaymentMethodRate>())
                .Select(r => new RateDisplayItem(r))
                .ToList();

            RatesCombo.ItemsSource = items;
            RatesCombo.SelectedItem = null;
            RatesCombo.IsVisible = true;
            SurchargeCheck.IsVisible = true;
            SurchargeCheck.IsChecked = false;
        }
        else
        {
            RatesCombo.IsVisible = false;
            SurchargeCheck.IsVisible = false;
            SurchargeBox.IsVisible = false;
        }

        RecalcAll();
    }

    private void RatesCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedRate = (RatesCombo.SelectedItem as RateDisplayItem)?.Rate;
        RecalcAll();
    }

    private void SurchargeCheck_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        RecalcAll();
    }

    private void AmountBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) TryConfirm();
        if (e.Key == Key.Escape) Close(null);
        RecalcAll();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => TryConfirm();
    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void RecalcAll()
    {
        decimal.TryParse((AmountBox.Text ?? "0").Replace(',', '.'),
            NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount);

        decimal surcharge = 0;
        decimal customerAmount = amount;

        if (_selectedMethod != null &&
            (_selectedMethod.Type == PaymentMethodType.DebitCard ||
             _selectedMethod.Type == PaymentMethodType.CreditCard) &&
            _selectedRate != null)
        {
            surcharge = Math.Round(amount * _selectedRate.RatePercentage / 100m, 2);
            SurchargeBox.IsVisible = true;
            SurchargeBox.Text = surcharge.ToString("N2", CultureInfo.CurrentCulture);

            if (SurchargeCheck.IsChecked == true)
                customerAmount = amount + surcharge;
        }
        else
        {
            SurchargeBox.IsVisible = false;
            SurchargeBox.Text = "0,00";
        }

        CustomerAmountText.Text = $"Valor a cobrar do cliente: R$ {customerAmount:N2}";
    }

    private void TryConfirm()
    {
        if (_selectedMethod == null)
        {
            SnackBarService.SendWarning("Selecione uma forma de pagamento.");
            return;
        }

        if (!decimal.TryParse((AmountBox.Text ?? "").Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
        {
            SnackBarService.SendWarning("Informe um valor válido maior que zero.");
            return;
        }

        decimal surcharge = 0;
        bool surchargeApplied = false;
        int installments = 1;

        bool isCard = _selectedMethod.Type == PaymentMethodType.DebitCard ||
                      _selectedMethod.Type == PaymentMethodType.CreditCard;

        if (isCard && _selectedRate != null)
        {
            installments = _selectedRate.Installments;
            surcharge = Math.Round(amount * _selectedRate.RatePercentage / 100m, 2);
            surchargeApplied = SurchargeCheck.IsChecked == true;
        }

        var result = (_selectedMethod, amount, installments, surchargeApplied,
            surchargeApplied ? surcharge : 0m);

        Close(result);
    }
}
