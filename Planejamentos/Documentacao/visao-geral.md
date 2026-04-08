# StoreSync — Visão Geral do Produto

## O que é

StoreSync é um sistema de gerenciamento para lojas de pequeno porte. Ele centraliza as operações do dia a dia em uma única plataforma: vendas, estoque, financeiro, funcionários e comissões.

## Problema que resolve

Lojas de pequeno porte costumam depender de planilhas, cadernos ou sistemas fragmentados para controlar vendas, estoque e caixa. Isso gera inconsistências, retrabalho e falta de visibilidade sobre o desempenho do negócio. O StoreSync unifica essas operações e oferece controle em tempo real.

## Público-alvo

Proprietários e gestores de lojas de pequeno porte com até ~20 funcionários.

## Funcionalidades

| Funcionalidade | Descrição resumida | Doc |
|---|---|---|
| Controle de Acesso | Login com JWT, perfis de permissão por papel | [acesso.md](acesso.md) |
| Cadastro de Funcionários | Registro de dados, CPF, cargo e taxa de comissão | [funcionarios.md](funcionarios.md) |
| Controle de Estoque | Produtos por categoria, quantidade e alertas de baixo estoque | [estoque.md](estoque.md) |
| PDV — Ponto de Venda | Registro de vendas com múltiplos itens e cálculo automático | [pdv.md](pdv.md) |
| Comissionamento | Cálculo automático de comissões por funcionário e período | [comissionamento.md](comissionamento.md) |
| Controle Financeiro | Lançamentos de receitas e despesas, status de pagamento | [financeiro.md](financeiro.md) |

## Stack técnica

- **Backend:** ASP.NET Core 9.0 (C#)
- **Banco de dados:** PostgreSQL 16
- **ORM:** Dapper (queries SQL diretas)
- **Autenticação:** JWT Bearer
- **Validação:** FluentValidation
- **Testes:** xUnit + Moq + FluentAssertions
- **Infraestrutura:** Docker + Docker Compose

## Arquitetura

```
Client (frontend / Postman / PDV)
        │
        ▼
   Controllers          ← HTTP, validação de entrada
        │
        ▼
    Services            ← Regras de negócio
        │
        ▼
  Repositories          ← Acesso ao banco via Dapper
        │
        ▼
   PostgreSQL
```

Padrão: Repository + Service Layer. Sem ORM pesado — queries SQL explícitas para performance e controle.

## Convenções do projeto

- Todos os IDs são `Guid`
- Datas em UTC
- Senhas hasheadas com BCrypt
- Migrations automáticas via `MigrationService` na inicialização
- TDD obrigatório: testes escritos antes da implementação
