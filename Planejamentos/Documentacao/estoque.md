# Controle de Estoque

## Entidades

**Category** — agrupa produtos.

| Campo | Tipo | Descrição |
|---|---|---|
| CategoryId | Guid | PK |
| Name | string | Nome (único) |
| CreatedAt | DateTime | Data de cadastro (UTC) |

**Product** — item vendável.

| Campo | Tipo | Descrição |
|---|---|---|
| ProductId | Guid | PK |
| Reference | string | Código de referência / EAN |
| Name | string | Nome |
| CategoryId | Guid | Categoria |
| Price | decimal | Preço de venda |
| StockQuantity | int | Quantidade em estoque |

## Endpoints

**Categorias:** `GET/POST /api/categories` · `GET/PUT/DELETE /api/categories/{id}`

**Produtos:** `GET/POST /api/products` · `GET/PUT/DELETE /api/products/{id}`

## Regras de negócio

- Nome de categoria é único → duplicata retorna `409 Conflict`
- `Price` > 0; `StockQuantity` ≥ 0
- Estoque é decrementado automaticamente ao finalizar venda
- Não é possível vender produto com estoque zero
- Não é possível deletar categoria com produtos vinculados
