# PDV — Ponto de Venda

## Objetivo

Registrar as vendas realizadas na loja. Cada venda é composta por um ou mais itens (produtos), vinculada a um funcionário, com cálculo automático do valor total.

## Entidades

### Sale (Venda)

| Campo | Tipo | Descrição |
|---|---|---|
| SaleId | Guid | Identificador único |
| EmployeeId | Guid | Funcionário que realizou a venda |
| TotalAmount | decimal | Valor total da venda |
| SaleDate | DateTime | Data e hora da venda (UTC) |

### SaleItem (Item da Venda)

| Campo | Tipo | Descrição |
|---|---|---|
| SaleItemId | Guid | Identificador único |
| SaleId | Guid | Venda à qual o item pertence |
| ProductId | Guid | Produto vendido |
| Quantity | int | Quantidade vendida |
| TotalPrice | decimal | Preço total do item (Quantity × preço unitário) |

## Endpoints — Vendas

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/sales` | Lista todas as vendas |
| GET | `/api/sales/{id}` | Busca venda por ID |
| POST | `/api/sales` | Registra nova venda |
| DELETE | `/api/sales/{id}` | Cancela venda |

## Endpoints — Itens de Venda

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/saleitems` | Lista todos os itens |
| GET | `/api/saleitems/{id}` | Busca item por ID |
| POST | `/api/saleitems` | Adiciona item a uma venda |
| DELETE | `/api/saleitems/{id}` | Remove item de uma venda |

## Fluxo de uma venda

```
1. Funcionário inicia a venda → POST /api/sales
        │
        ▼
2. Adiciona produtos um a um → POST /api/saleitems
        │  (cada item decrementa o estoque automaticamente)
        ▼
3. Venda finalizada com TotalAmount calculado
        │
        ▼
4. Sistema registra comissão para o funcionário (automático)
```

## Regras de negócio

- Uma venda deve ter pelo menos um item
- Não é possível vender produto com `StockQuantity = 0`
- `Quantity` por item deve ser maior que zero
- `TotalPrice` do item = `Quantity × Product.Price` (calculado no backend)
- `TotalAmount` da venda = soma dos `TotalPrice` dos itens
- Cancelar uma venda reverte o estoque dos itens
- Não é possível alterar uma venda já finalizada — apenas cancelar

## Relações

```
Sale ──── Employee     (N vendas → 1 funcionário)
Sale ──── SaleItem     (1 venda → N itens)
SaleItem ──── Product  (N itens → 1 produto)
```
