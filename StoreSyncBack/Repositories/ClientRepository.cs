using System.Data;
using Dapper;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly IDbConnection _db;

        public ClientRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<PaginatedResult<Client>> GetAllClientsAsync(int limit = 50, int offset = 0)
        {
            var countSql = "SELECT COUNT(*) FROM client;";
            var totalCount = await _db.ExecuteScalarAsync<int>(countSql);

            var sql = @"
                SELECT
                    client_id           AS ClientId,
                    reference           AS Reference,
                    name                AS Name,
                    cpf_cnpj            AS CpfCnpj,
                    phone               AS Phone,
                    email               AS Email,
                    address             AS Address,
                    address_number      AS AddressNumber,
                    address_complement  AS AddressComplement,
                    city                AS City,
                    state               AS State,
                    postal_code         AS PostalCode,
                    status              AS Status,
                    created_at          AS CreatedAt,
                    updated_at          AS UpdatedAt
                FROM client
                ORDER BY name
                LIMIT @Limit OFFSET @Offset;
            ";
            var result = await _db.QueryAsync<Client>(sql, new { Limit = limit, Offset = offset });

            return new PaginatedResult<Client>
            {
                Items = result,
                TotalCount = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        public async Task<Client?> GetClientByIdAsync(Guid clientId)
        {
            var sql = @"
                SELECT
                    client_id           AS ClientId,
                    reference           AS Reference,
                    name                AS Name,
                    cpf_cnpj            AS CpfCnpj,
                    phone               AS Phone,
                    email               AS Email,
                    address             AS Address,
                    address_number      AS AddressNumber,
                    address_complement  AS AddressComplement,
                    city                AS City,
                    state               AS State,
                    postal_code         AS PostalCode,
                    status              AS Status,
                    created_at          AS CreatedAt,
                    updated_at          AS UpdatedAt
                FROM client
                WHERE client_id = @Id;
            ";
            return await _db.QueryFirstOrDefaultAsync<Client?>(sql, new { Id = clientId });
        }

        public async Task<int> CreateClientAsync(Client client)
        {
            if (client.ClientId == Guid.Empty)
                client.ClientId = Guid.NewGuid();

            if (client.CreatedAt == default)
                client.CreatedAt = BrazilDateTime.Now;

            client.UpdatedAt = client.CreatedAt;

            var sql = @"
                INSERT INTO client (
                    client_id, name, cpf_cnpj, phone, email,
                    address, address_number, address_complement,
                    city, state, postal_code, status, created_at, updated_at
                )
                VALUES (
                    @ClientId, @Name, @CpfCnpj, @Phone, @Email,
                    @Address, @AddressNumber, @AddressComplement,
                    @City, @State, @PostalCode, @Status, @CreatedAt, @UpdatedAt
                )
                RETURNING reference;
            ";
            client.Reference = await _db.ExecuteScalarAsync<string>(sql, client);
            return 1;
        }

        public async Task<int> UpdateClientAsync(Client client)
        {
            client.UpdatedAt = BrazilDateTime.Now;

            var sql = @"
                UPDATE client
                SET
                    name               = @Name,
                    cpf_cnpj           = @CpfCnpj,
                    phone              = @Phone,
                    email              = @Email,
                    address            = @Address,
                    address_number     = @AddressNumber,
                    address_complement = @AddressComplement,
                    city               = @City,
                    state              = @State,
                    postal_code        = @PostalCode,
                    status             = @Status,
                    updated_at         = @UpdatedAt
                WHERE client_id = @ClientId;
            ";
            return await _db.ExecuteAsync(sql, new
            {
                client.Name,
                client.CpfCnpj,
                client.Phone,
                client.Email,
                client.Address,
                client.AddressNumber,
                client.AddressComplement,
                client.City,
                client.State,
                client.PostalCode,
                client.Status,
                client.UpdatedAt,
                client.ClientId
            });
        }

        public async Task<int> DeleteClientAsync(Guid clientId)
        {
            var sql = "DELETE FROM client WHERE client_id = @Id;";
            return await _db.ExecuteAsync(sql, new { Id = clientId });
        }
    }
}
