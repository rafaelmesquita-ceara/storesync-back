namespace SharedModels;

public class PaymentMethodRate
{
    public Guid    RateId          { get; set; }
    public Guid    PaymentMethodId { get; set; }
    public int     Installments    { get; set; }
    public decimal RatePercentage  { get; set; }
    public DateTime CreatedAt      { get; set; }
}
