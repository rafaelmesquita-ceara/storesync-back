namespace SharedModels;

public static class ClientStatus
{
    public const int Ativo = 1;
    public const int Inativo = 2;
    public const int Bloqueado = 3;
}

public class Client
{
    public Guid ClientId { get; set; }
    public string? Reference { get; set; }
    public string? Name { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? AddressNumber { get; set; }
    public string? AddressComplement { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int Status { get; set; } = ClientStatus.Ativo;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string StatusLabel => Status switch
    {
        ClientStatus.Ativo => "Ativo",
        ClientStatus.Inativo => "Inativo",
        ClientStatus.Bloqueado => "Bloqueado",
        _ => "Desconhecido"
    };
}
