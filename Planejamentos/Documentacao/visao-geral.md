# StoreSync — Visão Geral

Sistema de gestão para lojas de pequeno porte: vendas, estoque, financeiro, funcionários e comissões.

## Módulos

| Módulo | Descrição | Doc |
|---|---|---|
| Retaguarda (desktop) | Interface Avalonia para cadastros e movimentações | [retaguarda.md](retaguarda.md) |
| Controle de Acesso | Login JWT, perfis por papel | [acesso.md](acesso.md) |
| Funcionários | Dados, CPF, cargo, taxa de comissão | [funcionarios.md](funcionarios.md) |
| Estoque | Produtos por categoria, quantidade | [estoque.md](estoque.md) |
| PDV | Vendas com múltiplos itens | [pdv.md](pdv.md) |
| Comissionamento | Cálculo automático por funcionário/período | [comissionamento.md](comissionamento.md) |
| Financeiro | Contas a pagar/receber, liquidação | [financeiro.md](financeiro.md) |

## Convenções

- Todos os IDs são `Guid`; datas em UTC; senhas com BCrypt
- Migrations automáticas via `MigrationService` na inicialização
- TDD obrigatório: testes escritos antes da implementação
- Backend: ASP.NET Core 9 + PostgreSQL + Dapper (Repository + Service Layer)
- Frontend: Avalonia UI 11 + CommunityToolkit.Mvvm (MVVM)
