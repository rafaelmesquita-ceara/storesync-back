# Estoque

## Entidades

**Category:** `CategoryId (Guid PK)`, `Name (único)`, `CreatedAt`

**Product:** `ProductId (Guid PK)`, `Reference (EAN/código)`, `Name`, `CategoryId (FK)`, `Price (decimal >0)`, `StockQuantity (int ≥0)`

## Endpoints

- Categorias: `GET/POST /api/categories` · `GET/PUT/DELETE /api/categories/{id}`
- Produtos: `GET/POST /api/products` · `GET/PUT/DELETE /api/products/{id}`

## Regras

- Nome de categoria único → duplicata retorna 409
- Estoque decrementado ao finalizar venda; não vende com estoque zero
- Não deleta categoria com produtos vinculados
