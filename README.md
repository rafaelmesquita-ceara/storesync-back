# StoreSyncBack

API REST para gerenciamento de loja desenvolvida em ASP.NET Core 9.0.


## Tecnologias

- **.NET 9.0** - Framework principal
- **PostgreSQL** - Banco de dados
- **Dapper** - Micro-ORM para acesso a dados
- **JWT** - Autenticação via tokens
- **FluentValidation** - Validação de entradas
- **BCrypt** - Hash de senhas
- **Swagger** - Documentação da API

## Estrutura do Projeto

```
├── StoreSyncBack/          # API principal (Controllers, Services, Repositories)
│   ├── Controllers/         # Endpoints da API
│   ├── Services/            # Regras de negócio
│   ├── Repositories/        # Acesso a dados (Dapper)
│   ├── BD/                  # Scripts SQL
│   └── appsettings.json     # Configurações
├── SharedModels/            # Modelos compartilhados e interfaces
│   ├── Models/              # Entidades de domínio
│   └── Interfaces/          # Contratos (IUser, IProduct, etc.)
└── Planejamento/            # Documentação de planejamento
```

## Executando com Docker

O projeto inclui suporte completo a Docker com orquestração via Docker Compose.

### Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Iniciar a aplicação

Na raiz do projeto, execute:

```bash
docker-compose up -d
```

Ou para ver os logs em tempo real:

```bash
docker-compose up
```

### Acessar a aplicação

| Serviço | URL |
|---------|-----|
| API HTTP | http://localhost:5269 |
| Swagger UI | http://localhost:5269/swagger |
| PostgreSQL | localhost:5432 |

### Credenciais padrão

- **Banco de dados**: `store` / `storepass`
- **API**: usuário `admin` criado automaticamente (senha definida no script SQL)

### Comandos úteis

```bash
# Ver logs da API
docker-compose logs -f api

# Ver logs do banco
docker-compose logs -f db

# Parar os containers
docker-compose down

# Parar e remover volumes (limpa dados do banco)
docker-compose down -v

# Rebuildar após alterações no código
docker-compose up -d --build

# Executar comandos no container da API
docker-compose exec api dotnet --version

# Acessar o banco via psql
docker-compose exec db psql -U store -d storesync
```

## Executando localmente (sem Docker)

### Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL 14+ rodando localmente

### Configurar o banco de dados

1. Crie o banco de dados `storesync` no PostgreSQL
2. Execute os scripts em `StoreSyncBack/BD/` para criar as tabelas
3. Execute `inserir_root.sql` para criar o usuário admin

### Configurar a connection string

Edite `StoreSyncBack/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=storesync;Username=store;Password=storepass"
  }
}
```

### Rodar a API

```bash
# Build
dotnet build StoreSyncBack/StoreSyncBack.sln

# Run
dotnet run --project StoreSyncBack/StoreSyncBack.csproj
```

A API estará disponível em:
- HTTP: http://localhost:5269
- HTTPS: https://localhost:7044
- Swagger: http://localhost:5269/swagger

## Arquitetura

A API segue o padrão Repository com camada de serviço:

```
Controllers -> Services -> Repositories -> Database (PostgreSQL/Dapper)
```

Cada entidade possui:
- **Model**: Definição dos dados (`SharedModels/Models/`)
- **Interface**: Contratos de repository e service (`SharedModels/Interfaces/`)
- **Repository**: Acesso a dados usando Dapper (`StoreSyncBack/Repositories/`)
- **Service**: Regras de negócio (`StoreSyncBack/Services/`)
- **Controller**: Endpoints HTTP (`StoreSyncBack/Controllers/`)

## Entidades

Todas as entidades usam `Guid` como chave primária:

- **User** - Autenticação (login, senha), vinculado a Employee
- **Employee** - Funcionários (nome, CPF, role, taxa de comissão)
- **Category** - Categorias de produtos
- **Product** - Produtos da loja
- **Sale** - Vendas
- **SaleItem** - Itens das vendas
- **Commission** - Comissões dos funcionários
- **Finance** - Registros financeiros

## Autenticação

A API usa JWT Bearer tokens. Para obter um token:

```bash
curl -X POST http://localhost:5269/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"login": "admin", "password": "admin"}'
```

Use o token retornado nas requisições subsequentes:

```bash
curl http://localhost:5269/api/products \
  -H "Authorization: Bearer {seu_token}"
```

## Validação na Inicialização

A API verifica automaticamente se o banco de dados está acessível antes de iniciar. Se a conexão falhar, a aplicação encerra com erro e não inicia os endpoints.
