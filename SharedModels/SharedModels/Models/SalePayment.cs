namespace SharedModels;

public class SalePayment
{
    public Guid    SalePaymentId    { get; set; }
    public Guid    SaleId           { get; set; }
    public Guid    PaymentMethodId  { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal Amount           { get; set; }
    public int     Installments     { get; set; } = 1;
    public bool    SurchargeApplied { get; set; }
    public decimal SurchargeAmount  { get; set; }
    public DateTime CreatedAt       { get; set; }

    public string PaymentMethodName => PaymentMethod?.Name ?? string.Empty;
}
