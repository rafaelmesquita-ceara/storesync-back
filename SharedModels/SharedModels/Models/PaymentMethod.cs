namespace SharedModels;

public static class PaymentMethodType
{
    public const int Cash       = 1;
    public const int DebitCard  = 2;
    public const int CreditCard = 3;
    public const int Pix        = 4;
}

public static class PaymentMethodStatus
{
    public const int Ativo   = 1;
    public const int Inativo = 0;
}

public class PaymentMethod
{
    public Guid   PaymentMethodId { get; set; }
    public string Name            { get; set; } = string.Empty;
    public int    Type            { get; set; }
    public int    Status          { get; set; } = PaymentMethodStatus.Ativo;
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
    public List<PaymentMethodRate>? Rates { get; set; }

    public string TypeLabel => Type switch
    {
        PaymentMethodType.Cash       => "Dinheiro",
        PaymentMethodType.DebitCard  => "Débito",
        PaymentMethodType.CreditCard => "Crédito",
        PaymentMethodType.Pix        => "Pix",
        _                            => "Desconhecido"
    };
}
