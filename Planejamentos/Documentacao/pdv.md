# PDV — Ponto de Venda

## Entidades

**Sale (Venda)**

| Campo | Tipo | Descrição |
|---|---|---|
| SaleId | Guid | PK |
| EmployeeId | Guid | Funcionário responsável |
| Discount | decimal | Desconto geral |
| Addition | decimal | Acréscimo geral |
| TotalAmount | decimal | Soma dos itens - Discount + Addition |
| Status | int | 1=Aberta, 2=Finalizada, 3=Cancelada |
| SaleDate | DateTime | Data/hora UTC |

**SaleItem (Item da Venda)**

| Campo | Tipo | Descrição |
|---|---|---|
| SaleItemId | Guid | PK |
| SaleId | Guid | Venda |
| ProductId | Guid | Produto |
| Quantity | int | Quantidade |
| Discount | decimal | Desconto no item |
| Addition | decimal | Acréscimo no item |
| TotalPrice | decimal | Quantity × preço unitário - Discount + Addition |

## Endpoints — Vendas

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/sales` | Lista vendas |
| GET | `/api/sales/{id}` | Busca por ID (inclui itens) |
| POST | `/api/sales` | Cria venda (status Aberta, sem itens) |
| PUT | `/api/sales/{id}` | Atualiza (apenas se Aberta) |
| POST | `/api/sales/{id}/finalize` | Finaliza (abate estoque) |
| POST | `/api/sales/{id}/cancel` | Cancela (reverte estoque se Finalizada) |

## Endpoints — Itens

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/saleitems/by-sale/{saleId}` | Lista itens de uma venda |
| POST | `/api/saleitems` | Adiciona item (valida estoque) |
| PUT | `/api/saleitems/{id}` | Atualiza item |
| DELETE | `/api/saleitems/{id}` | Remove item (venda deve ser Aberta) |

## Regras de negócio

- `TotalPrice` do item = `Quantity × Product.Price - Discount + Addition` (calculado no backend)
- `TotalAmount` da venda = soma dos `TotalPrice` - Discount da venda + Addition da venda
- Não é possível vender produto com `StockQuantity` insuficiente; `Quantity` > 0
- Para finalizar: venda deve ter pelo menos 1 item
- Ao finalizar: estoque abatido; ao cancelar Finalizada: estoque revertido; ao cancelar Aberta: sem alteração de estoque
- Não é possível excluir venda (apenas cancelar); não é possível editar Finalizada ou Cancelada
- Funcionário pré-preenchido com o funcionário do usuário logado

## Relações

```
Sale → Employee   (N vendas → 1 funcionário)
Sale → SaleItem   (1 venda → N itens)
SaleItem → Product
```
