# Clientes

## Entidade: Client

`ClientId (Guid PK)`, `Reference (auto: CLI00001…)`, `Name (obrigatório)`, `CpfCnpj? (único)`, `Phone?`, `Email? (único)`, `Address?`, `AddressNumber?`, `AddressComplement?`, `City?`, `State (CHAR 2)`, `PostalCode?`, `Status (1=Ativo 2=Inativo 3=Bloqueado, padrão 1)`, `CreatedAt`, `UpdatedAt`

## Endpoints

`GET/POST /api/clients` · `GET/PUT/DELETE /api/clients/{id}` · `GET /api/clients?limit=&offset=`

## Regras

- `Name` obrigatório; `CpfCnpj` e `Email` únicos (banco retorna erro em duplicata)
- `Reference` gerada por sequence no banco; não enviar no POST
- `UpdatedAt` atualizado automaticamente; exclusão física
- Vínculo com `sale.client_id` opcional; `ON DELETE SET NULL`
- Sem validação de formato CPF/CNPJ no backend (só unicidade)

## Frontend

Menu: `Cadastros → Clientes`. Grid: Cód., Nome, CPF/CNPJ, Telefone, E-mail, Cidade, Status. Busca client-side normalizada (sem acento, case-insensitive, múltiplos termos). Paginação 50/página. Atalhos: F1 (novo), F5 (recarregar), Enter (salvar), Esc (cancelar). Formulário único criação/edição controlado por `IsEdit`.

## Migration relevante

`006_clients.sql`: `ALTER TABLE sale ADD COLUMN IF NOT EXISTS client_id UUID REFERENCES client(client_id) ON DELETE SET NULL;`
