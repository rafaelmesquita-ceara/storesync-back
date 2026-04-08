# Controle de Estoque

## Objetivo

Gerenciar os produtos disponíveis na loja, organizados por categoria, com controle de quantidade em estoque.

## Entidades

### Category

Agrupa os produtos por tipo.

| Campo | Tipo | Descrição |
|---|---|---|
| CategoryId | Guid | Identificador único |
| Name | string | Nome da categoria (único) |
| CreatedAt | DateTime | Data de cadastro (UTC) |

### Product

Representa um item vendável da loja.

| Campo | Tipo | Descrição |
|---|---|---|
| ProductId | Guid | Identificador único |
| Reference | string | Código de referência / EAN |
| Name | string | Nome do produto |
| CategoryId | Guid | Categoria do produto |
| Price | decimal | Preço de venda |
| StockQuantity | int | Quantidade em estoque |

## Endpoints — Categorias

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/categories` | Lista todas as categorias |
| GET | `/api/categories/{id}` | Busca categoria por ID |
| POST | `/api/categories` | Cria categoria |
| PUT | `/api/categories/{id}` | Atualiza categoria |
| DELETE | `/api/categories/{id}` | Remove categoria |

## Endpoints — Produtos

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/products` | Lista todos os produtos |
| GET | `/api/products/{id}` | Busca produto por ID |
| POST | `/api/products` | Cadastra produto |
| PUT | `/api/products/{id}` | Atualiza produto |
| DELETE | `/api/products/{id}` | Remove produto |

## Regras de negócio

- Nome de categoria é único (constraint no banco)
- Tentativa de criar categoria duplicada retorna `409 Conflict`
- `Price` deve ser maior que zero
- `StockQuantity` não pode ser negativo
- Ao registrar uma venda, o estoque é decrementado automaticamente
- Não é possível vender produto com estoque zero
- Não é possível deletar categoria que tenha produtos vinculados

## Movimentação de estoque

O estoque é atualizado automaticamente pelo módulo de vendas (PDV). Não há endpoint manual de entrada/saída de estoque neste momento — toda movimentação ocorre via venda.

> **Evolução futura:** entrada manual de estoque (nota fiscal / reposição) é um ponto de extensão planejado.
