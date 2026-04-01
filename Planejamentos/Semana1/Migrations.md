# Planejamento: Sistema de Migrations para Dapper

## Visão Geral

Implementar um sistema de migrations personalizado para o StoreSyncBack que executa automaticamente scripts SQL versionados na inicialização da aplicação, garantindo que o banco de dados esteja sempre atualizado.

---

## Componentes do Sistema

### 1. Tabela de Controle (`historico_versao`)

Responsável por rastrear quais migrations já foram aplicadas.

**Estrutura:**
```sql
CREATE TABLE historico_versao (
    id SERIAL PRIMARY KEY,
    numero_release VARCHAR(20) NOT NULL UNIQUE,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

**Campos:**
- `id` - Identificador único (auto-incremento)
- `numero_release` - Identificador da migration (ex: "000", "001", "001_seed_data")
- `data_atualizacao` - Data/hora em que a migration foi aplicada

---

### 2. Estrutura de Arquivos de Migration

**Diretório:** `StoreSyncBack/Migrations/`

**Padrão de nomenclatura:**
```
Migrations/
├── 000_initial_schema.sql    -- Criação de todas as tabelas
├── 001_seed_data.sql         -- Dados iniciais (opcional)
├── 002_add_indexes.sql       -- Alterações futuras
└── ...
```

**Regras:**
- Nome do arquivo: `{numero_release}_{descricao}.sql`
- `numero_release` deve ser único e ordenável
- Scripts devem ser idempotentes quando possível
- Usar transações para garantir atomicidade

---

### 3. Serviço de Migration (`MigrationService`)

**Responsabilidades:**
1. Verificar a versão atual do banco (última migration aplicada)
2. Identificar migrations pendentes (arquivos no diretório > versão atual)
3. Executar migrations em ordem dentro de uma transação
4. Registrar cada migration aplicada na tabela `historico_versao`
5. Logar o progresso das migrations

**Algoritmo:**
```csharp
1. Criar tabela historico_versao se não existir
2. Obter última versão aplicada (SELECT MAX(numero_release) FROM historico_versao)
3. Listar arquivos .sql em Migrations/ ordenados por nome
4. Para cada arquivo onde numero_release > ultima_versao:
   a. Iniciar transação
   b. Executar conteúdo do arquivo SQL
   c. INSERT INTO historico_versao (numero_release, data_atualizacao)
   d. Commit da transação
   e. Log: "Migration {numero_release} aplicada com sucesso"
```

**Interface:**
```csharp
public interface IMigrationService
{
    Task ApplyMigrationsAsync();
}
```

---

### 4. Primeira Migration (`000_initial_schema.sql`)

**Responsabilidade:** Criar todas as 8 tabelas do sistema + tabela de histórico.

**Tabelas a criar:**
1. `employee` - Funcionários
2. `"user"` - Usuários de login (nome entre aspas - palavra reservada)
3. `category` - Categorias de produtos
4. `product` - Produtos
5. `sale` - Vendas
6. `sale_item` - Itens de venda
7. `commission` - Comissões
8. `finance` - Registros financeiros
9. `historico_versao` - Controle de migrations

**Script SQL completo:**
```sql
-- Tabela: employee
CREATE TABLE IF NOT EXISTS employee (
    employee_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    cpf VARCHAR(14) UNIQUE NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'user',
    commission_rate DECIMAL(10,2) DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: "user" (entre aspas - palavra reservada do PostgreSQL)
CREATE TABLE IF NOT EXISTS "user" (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    login VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    employee_id UUID NOT NULL REFERENCES employee(employee_id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: category
CREATE TABLE IF NOT EXISTS category (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: product
CREATE TABLE IF NOT EXISTS product (
    product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    reference VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(200) NOT NULL,
    category_id UUID REFERENCES category(category_id) ON DELETE SET NULL,
    price DECIMAL(12,2) NOT NULL DEFAULT 0,
    stock_quantity INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: sale
CREATE TABLE IF NOT EXISTS sale (
    sale_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL REFERENCES employee(employee_id),
    total_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    sale_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: sale_item
CREATE TABLE IF NOT EXISTS sale_item (
    sale_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sale_id UUID NOT NULL REFERENCES sale(sale_id) ON DELETE CASCADE,
    product_id UUID NOT NULL REFERENCES product(product_id),
    quantity INTEGER NOT NULL DEFAULT 1,
    total_price DECIMAL(12,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela: commission
CREATE TABLE IF NOT EXISTS commission (
    commission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL REFERENCES employee(employee_id) ON DELETE CASCADE,
    month DATE NOT NULL,
    total_sales DECIMAL(12,2) NOT NULL DEFAULT 0,
    commission_value DECIMAL(12,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(employee_id, month)
);

-- Tabela: finance
CREATE TABLE IF NOT EXISTS finance (
    finance_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    description TEXT NOT NULL,
    amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    due_date DATE NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de controle de migrations (criada por último para garantir existência)
CREATE TABLE IF NOT EXISTS historico_versao (
    id SERIAL PRIMARY KEY,
    numero_release VARCHAR(20) NOT NULL UNIQUE,
    data_atualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

---

### 5. Configuração do Docker (Volume Persistente)

**Problema atual:** O volume `./StoreSyncBack/BD:/docker-entrypoint-initdb.d` monta scripts de inicialização, mas não persiste os dados do banco entre reinicializações.

**Solução:** Adicionar um volume nomeado para os dados do PostgreSQL.

**Arquivo:** `docker-compose.yml`

```yaml
version: '3.8'

services:
  db:
    image: postgres:16-alpine
    container_name: storesync_db
    environment:
      POSTGRES_DB: storesync
      POSTGRES_USER: store
      POSTGRES_PASSWORD: storepass
    ports:
      - "5432:5432"
    volumes:
      # Volume para scripts de inicialização (criação de banco/usuário)
      - ./StoreSyncBack/BD:/docker-entrypoint-initdb.d:ro
      # Volume NOMEADO para persistir dados entre reinicializações
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U store -d storesync"]
      interval: 5s
      timeout: 5s
      retries: 5

  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: storesync_api
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=storesync;Username=store;Password=storepass"
    ports:
      - "5269:8080"
      - "7044:8081"
    depends_on:
      db:
        condition: service_healthy
    command: >
      sh -c "dotnet ef database migrate || true && 
             dotnet StoreSyncBack.dll"

# Declaração do volume nomeado
volumes:
  postgres_data:
    driver: local
```

**Benefícios do volume nomeado:**
- Dados persistem mesmo após `docker-compose down`
- Para resetar: `docker-compose down -v` (remove volumes)
- Para backup: pode-se copiar do volume para arquivo

---

## Fluxo de Inicialização da Aplicação

```
┌─────────────────────────────────────────────────────────────┐
│                     StoreSyncBack API                       │
│                      Inicialização                          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  1. Program.cs - Configuração de serviços                  │
│     - Registrar IDbConnection                               │
│     - Registrar IMigrationService                           │
│     - Registrar outros serviços/repos                       │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  2. Verificação de conexão com banco                       │
│     - Tentar conectar ao PostgreSQL                         │
│     - Se falhar: logar erro e encerrar                      │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Executar MigrationService.ApplyMigrationsAsync()        │
│     - Criar tabela historico_versao se não existir          │
│     - Identificar migrations pendentes                      │
│     - Aplicar cada migration em transação                   │
│     - Registrar na tabela de histórico                      │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Continuar inicialização normal da API                   │
│     - Configurar pipeline HTTP                              │
│     - Iniciar servidor                                      │
│     - Disponibilizar endpoints                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Arquivos a Criar/Modificar

### Novos Arquivos:

| Arquivo | Descrição |
|---------|-----------|
| `StoreSyncBack/Services/IMigrationService.cs` | Interface do serviço |
| `StoreSyncBack/Services/MigrationService.cs` | Implementação do serviço |
| `StoreSyncBack/Migrations/000_initial_schema.sql` | Criação das tabelas |
| `StoreSyncBack/Migrations/001_seed_root_user.sql` | Dados iniciais (mover de inserir_root.sql) |

### Arquivos a Modificar:

| Arquivo | Modificação |
|---------|-------------|
| `StoreSyncBack/Program.cs` | Registrar MigrationService e chamar ApplyMigrationsAsync na inicialização |
| `docker-compose.yml` | Adicionar volume nomeado `postgres_data` |
| `StoreSyncBack/BD/inserir_root.sql` | Mover para Migrations/001_seed_root.sql ou remover |

---

## Testes e Validação

### Cenários de Teste:

1. **Primeira inicialização (banco vazio):**
   - Subir docker-compose
   - Verificar se tabela `historico_versao` foi criada
   - Verificar se todas as 8 tabelas foram criadas
   - Verificar se migration "000" foi registrada

2. **Reinicialização (banco já existe):**
   - Parar e subir containers novamente
   - Verificar se migrations não são reaplicadas (idempotência)
   - Verificar se dados persistem

3. **Nova migration:**
   - Criar arquivo `002_test_migration.sql`
   - Reiniciar API
   - Verificar se apenas migration 002 foi aplicada
   - Verificar registro na tabela historico_versao

4. **Falha em migration:**
   - Criar migration com SQL inválido
   - Verificar se transação faz rollback
   - Verificar se aplicação não inicia com banco inconsistente

---

## Considerações de Segurança

1. **Backup:** Antes de aplicar migrations em produção, fazer backup do banco
2. **Transações:** Cada migration deve rodar dentro de uma transação
3. **Idempotência:** Scripts devem usar `IF NOT EXISTS` quando possível
4. **Ordem:** Migrations são aplicadas em ordem alfabética/númerica
5. **Concorrência:** Sistema deve suportar múltiplas instâncias (bloqueio/lock se necessário)

---

## Exemplo de Uso

**Para adicionar uma nova migration no futuro:**

1. Criar arquivo `StoreSyncBack/Migrations/002_add_product_description.sql`:
```sql
-- Adicionar coluna description na tabela product
ALTER TABLE product ADD COLUMN IF NOT EXISTS description TEXT;
```

2. Reiniciar a aplicação

3. Verificar log:
```
[2025-01-15 10:30:45] MigrationService: Aplicando migration 002_add_product_description.sql...
[2025-01-15 10:30:45] MigrationService: Migration 002 aplicada com sucesso
```

4. Consultar tabela de histórico:
```sql
SELECT * FROM historico_versao ORDER BY numero_release;
-- Resultado:
-- id | numero_release | data_atualizacao
--  1 | 000            | 2025-01-15 10:30:00
--  2 | 001            | 2025-01-15 10:30:00
--  3 | 002            | 2025-01-15 10:30:45
```

---

## Próximos Passos

1. Implementar `IMigrationService` e `MigrationService`
2. Criar pasta `Migrations/` e adicionar `000_initial_schema.sql`
3. Modificar `Program.cs` para executar migrations na inicialização
4. Atualizar `docker-compose.yml` com volume persistente
5. Testar cenários descritos acima
6. Documentar no CLAUDE.md como adicionar novas migrations
