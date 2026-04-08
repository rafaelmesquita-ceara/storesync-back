# Controle Financeiro

## Objetivo

Registrar e acompanhar as movimentações financeiras da loja: receitas, despesas e contas a pagar/receber, com controle de status de pagamento.

## Entidade: Finance

| Campo | Tipo | Descrição |
|---|---|---|
| FinanceId | Guid | Identificador único |
| Description | string | Descrição do lançamento |
| Amount | decimal | Valor (positivo = receita, negativo = despesa) |
| DueDate | DateTime | Data de vencimento |
| Status | string | Status do lançamento |

### Status possíveis

| Status | Descrição |
|---|---|
| `pending` | Pendente — ainda não pago/recebido |
| `paid` | Pago / recebido |
| `overdue` | Vencido — prazo expirado sem pagamento |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/finances` | Lista todos os lançamentos |
| GET | `/api/finances/{id}` | Busca lançamento por ID |
| POST | `/api/finances` | Cria novo lançamento |
| PUT | `/api/finances/{id}` | Atualiza lançamento |
| DELETE | `/api/finances/{id}` | Remove lançamento |

## Regras de negócio

- `Amount` deve ser diferente de zero
- `Description` é obrigatória
- `DueDate` é obrigatória
- Status padrão ao criar: `pending`
- Um lançamento `paid` não pode ser deletado, apenas estornado (novo lançamento contrário)
- Lançamentos com `DueDate` no passado e status `pending` devem ser marcados como `overdue` (atualização periódica ou no momento da consulta)

## Tipos de lançamento (por convenção)

| Amount | Tipo |
|---|---|
| Positivo | Receita (ex: venda, aporte) |
| Negativo | Despesa (ex: aluguel, fornecedor) |

## Relações

O módulo financeiro é independente das vendas neste momento. As vendas não geram lançamentos financeiros automaticamente — os registros são manuais.

> **Evolução futura:** integração automática entre venda concluída e lançamento financeiro de receita, relatório de fluxo de caixa por período, DRE simplificado.
