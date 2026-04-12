# CLAUDE.md

## Projeto

**StoreSync** — gestão para lojas de pequeno porte.

- Backend: `StoreSyncBack/` — ASP.NET Core 9 + PostgreSQL + Dapper (Controllers → Services → Repositories)
- Frontend: `StoreSyncFront/` — Avalonia UI 11 + MVVM
- Modelos/interfaces/validators: `SharedModels/`
- Migrations SQL: `StoreSyncBack/Migrations/`

PKs: `Guid` · Datas: `BrazilDateTime.Now` (America/Sao_Paulo) · Senhas: BCrypt

## Comandos

```bash
dotnet build StoreSyncBack/StoreSyncBack.sln
dotnet run --project StoreSyncBack/StoreSyncBack.csproj   # http://localhost:5269
dotnet test StoreSyncBack.Tests/StoreSyncBack.Tests.csproj
dotnet run --project StoreSyncFront/StoreSyncFront.csproj
```

Swagger: `http://localhost:5269/swagger`

## Notas importantes

- Tabela `"user"` requer aspas duplas no PostgreSQL (palavra reservada)
- JWT contém claims `UserId` e `Role`; controllers nunca retornam `Password`
- Mensagens de log/validação em português
- Commit: resumo objetivo em português

## Migrations

`StoreSyncBack/Migrations/{versão}_{desc}.sql` (ex: `002_desc.sql`). Aplicadas automaticamente via `MigrationService`. Histórico em `historico_versao`. Usar SQL idempotente (`IF NOT EXISTS`).

## TDD (obrigatório)

- Escrever teste ANTES da implementação — xUnit + Moq + FluentAssertions
- `dotnet test` antes de cada commit; cobertura mínima 80% dos services
- Testes em `StoreSyncBack.Tests/Unit/Services/`; nomenclatura: `[Metodo]_[Estado]_[ResultadoEsperado]`
- Fixtures em `StoreSyncBack.Tests/Fixtures/`; mockar repositórios com Moq

## Docs por módulo (ler só o relevante ao implementar)

| Módulo | Arquivo |
|---|---|
| Visão geral | `Planejamentos/Documentacao/visao-geral.md` |
| Retaguarda (frontend) | `Planejamentos/Documentacao/retaguarda.md` |
| Acesso / Auth | `Planejamentos/Documentacao/acesso.md` |
| Estoque / Produtos | `Planejamentos/Documentacao/estoque.md` |
| Financeiro | `Planejamentos/Documentacao/financeiro.md` |
| Funcionários | `Planejamentos/Documentacao/funcionarios.md` |
| Comissionamento | `Planejamentos/Documentacao/comissionamento.md` |
| PDV / Vendas | `Planejamentos/Documentacao/pdv.md` |
| Clientes | `Planejamentos/Documentacao/clientes.md` |
