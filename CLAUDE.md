# CLAUDE.md

## Documentação por Módulo

Antes de implementar ou alterar qualquer módulo, leia o doc correspondente:

| Módulo | Arquivo |
|---|---|
| Visão geral | `Planejamentos/Documentacao/visao-geral.md` |
| Retaguarda (frontend) | `Planejamentos/Documentacao/retaguarda.md` |
| Acesso / Auth | `Planejamentos/Documentacao/acesso.md` |
| Estoque / Produtos / Categorias | `Planejamentos/Documentacao/estoque.md` |
| Financeiro | `Planejamentos/Documentacao/financeiro.md` |
| Funcionários | `Planejamentos/Documentacao/funcionarios.md` |
| Comissionamento | `Planejamentos/Documentacao/comissionamento.md` |
| PDV / Vendas | `Planejamentos/Documentacao/pdv.md` |

## Projeto

**StoreSync** — sistema de gestão para lojas de pequeno porte.

- Backend: `StoreSyncBack/` — ASP.NET Core 9 + PostgreSQL + Dapper (Controllers → Services → Repositories)
- Frontend: `StoreSyncFront/` — Avalonia UI (MVVM)
- Modelos/interfaces/validators: `SharedModels/`
- Migrations SQL: `StoreSyncBack/Migrations/`

Todas as PKs são `Guid`. Datas em horário de Brasília (`BrazilDateTime.Now` — `America/Sao_Paulo`). Senhas com BCrypt.

## Comandos

```bash
dotnet build StoreSyncBack/StoreSyncBack.sln
dotnet run --project StoreSyncBack/StoreSyncBack.csproj        # http://localhost:5269
dotnet run --project StoreSyncBack/StoreSyncBack.csproj --launch-profile https
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj
dotnet run --project StoreSyncFront/StoreSyncFront.csproj
```

Swagger: `http://localhost:5269/swagger`

## Notas importantes

- Tabela `"user"` requer aspas duplas no PostgreSQL (palavra reservada)
- JWT contém claims `UserId` e `Role`; controllers nunca retornam `Password`
- Mensagens de log/validação em português
- Mensagem de commit: resumo objetivo em português

## Migrations

Arquivos em `StoreSyncBack/Migrations/{versão}_{desc}.sql` (ex: `002_desc.sql`). Aplicadas automaticamente no startup via `MigrationService`. Histórico em tabela `historico_versao`. Usar SQL idempotente (`IF NOT EXISTS`).

## TDD (obrigatório)

- Escrever teste ANTES da implementação — xUnit + Moq + FluentAssertions
- `dotnet test` antes de cada commit; cobertura mínima 80% dos services
- Testes em `StoreSyncBack.Tests/Unit/Services/`; nomenclatura: `[Metodo]_[Estado]_[ResultadoEsperado]`
- Usar `TestData` fixture (`StoreSyncBack.Tests/Fixtures/`) para dados fake; mockar repositórios com Moq
