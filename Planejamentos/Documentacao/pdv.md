# PDV — Ponto de Venda

## Entidades

**Sale:** `SaleId (Guid PK)`, `EmployeeId (FK)`, `ClientId? (FK)`, `Discount`, `Addition`, `TotalAmount (Σitens - Discount + Addition)`, `Status (1=Aberta 2=Finalizada 3=Cancelada)`, `SaleDate`

**SaleItem:** `SaleItemId (Guid PK)`, `SaleId (FK)`, `ProductId (FK)`, `Quantity`, `Discount`, `Addition`, `TotalPrice (Qty × Price - Discount + Addition)`

## Endpoints

**Vendas:**
- `GET /api/sales` · `GET /api/sales/{id}` (inclui itens)
- `POST /api/sales` — cria Aberta sem itens
- `PUT /api/sales/{id}` — só se Aberta
- `POST /api/sales/{id}/finalize` — abate estoque
- `POST /api/sales/{id}/cancel` — reverte estoque se Finalizada

**Itens:**
- `GET /api/saleitems/by-sale/{saleId}`
- `POST /api/saleitems` — valida estoque
- `PUT /api/saleitems/{id}`
- `DELETE /api/saleitems/{id}` — venda deve ser Aberta

## Regras

- TotalPrice e TotalAmount calculados no backend
- Não vende com estoque insuficiente; Quantity > 0
- Finalizar exige ≥ 1 item
- Não exclui venda (só cancela); não edita Finalizada/Cancelada
- Funcionário pré-preenchido com o do usuário logado
