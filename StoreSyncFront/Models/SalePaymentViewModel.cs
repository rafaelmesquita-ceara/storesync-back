using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SharedModels;

public partial class SalePaymentViewModel : ObservableObject
{
    public SalePayment Model { get; }

    [ObservableProperty] private Guid salePaymentId;
    [ObservableProperty] private Guid saleId;
    [ObservableProperty] private Guid paymentMethodId;
    [ObservableProperty] private string paymentMethodName = string.Empty;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private int installments;
    [ObservableProperty] private bool surchargeApplied;
    [ObservableProperty] private decimal surchargeAmount;
    [ObservableProperty] private DateTime createdAt;

    public SalePaymentViewModel(SalePayment payment)
    {
        Model = payment;

        SalePaymentId    = payment.SalePaymentId;
        SaleId           = payment.SaleId;
        PaymentMethodId  = payment.PaymentMethodId;
        PaymentMethodName = payment.PaymentMethod?.Name ?? string.Empty;
        Amount           = payment.Amount;
        Installments     = payment.Installments;
        SurchargeApplied = payment.SurchargeApplied;
        SurchargeAmount  = payment.SurchargeAmount;
        CreatedAt        = payment.CreatedAt;
    }
}
