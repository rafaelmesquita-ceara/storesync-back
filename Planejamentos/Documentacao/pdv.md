# PDV — Ponto de Venda

## Objetivo

Registrar as vendas realizadas na loja. Cada venda é composta por um ou mais itens (produtos), vinculada a um funcionário, com cálculo automático do valor total, suporte a desconto/acréscimo por item e por venda, e controle de situação (Aberta, Finalizada, Cancelada).

## Entidades

### Sale (Venda)

| Campo | Tipo | Descrição |
|---|---|---|
| SaleId | Guid | Identificador único |
| EmployeeId | Guid | Funcionário que realizou a venda |
| Discount | decimal | Desconto geral da venda |
| Addition | decimal | Acréscimo geral da venda |
| TotalAmount | decimal | Valor total da venda (soma dos itens - desconto + acréscimo) |
| Status | int | Situação: 1=Aberta, 2=Finalizada, 3=Cancelada |
| SaleDate | DateTime | Data e hora da venda (UTC) |

### SaleItem (Item da Venda)

| Campo | Tipo | Descrição |
|---|---|---|
| SaleItemId | Guid | Identificador único |
| SaleId | Guid | Venda à qual o item pertence |
| ProductId | Guid | Produto vendido |
| Quantity | int | Quantidade vendida |
| Discount | decimal | Desconto no item |
| Addition | decimal | Acréscimo no item |
| TotalPrice | decimal | Preço total do item (Quantity × preço unitário - Discount + Addition) |

## Situações da Venda (SaleStatus)

| Valor | Label | Descrição |
|---|---|---|
| 1 | Aberta | Venda em andamento, permite edição e adição/remoção de itens |
| 2 | Finalizada | Venda concluída, estoque abatido, não permite edição |
| 3 | Cancelada | Venda cancelada, estoque revertido (se estava finalizada) |

## Endpoints — Vendas

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/sales` | Lista todas as vendas |
| GET | `/api/sales/{id}` | Busca venda por ID (inclui itens) |
| POST | `/api/sales` | Cria nova venda (status Aberta, sem itens) |
| PUT | `/api/sales/{id}` | Atualiza venda (apenas se Aberta) |
| POST | `/api/sales/{id}/finalize` | Finaliza venda (abate estoque) |
| POST | `/api/sales/{id}/cancel` | Cancela venda (reverte estoque se Finalizada) |

## Endpoints — Itens de Venda

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/saleitems` | Lista todos os itens |
| GET | `/api/saleitems/{id}` | Busca item por ID |
| GET | `/api/saleitems/by-sale/{saleId}` | Lista itens de uma venda |
| POST | `/api/saleitems` | Adiciona item a uma venda (valida estoque) |
| PUT | `/api/saleitems/{id}` | Atualiza item |
| DELETE | `/api/saleitems/{id}` | Remove item (apenas se venda Aberta) |

## Fluxo de uma venda

```
1. Funcionário inicia a venda → POST /api/sales (status=Aberta, sem itens)
        │
        ▼
2. Adiciona produtos um a um → POST /api/saleitems
        │  (valida estoque antes de adicionar)
        │  (recalcula TotalAmount da venda automaticamente)
        ▼
3. Pode fechar a tela e retomar depois (venda fica Aberta)
        │
        ▼
4. Finaliza a venda → POST /api/sales/{id}/finalize
        │  (abate estoque dos produtos vendidos)
        │  (muda status para Finalizada)
        ▼
5. Se necessário, cancela → POST /api/sales/{id}/cancel
        │  (reverte estoque se estava Finalizada)
        │  (muda status para Cancelada)
```

## Regras de negócio

- Uma venda começa com status Aberta e sem itens obrigatórios
- Não é possível vender produto com `StockQuantity` insuficiente
- `Quantity` por item deve ser maior que zero
- `TotalPrice` do item = `Quantity × Product.Price - Discount + Addition` (calculado no backend)
- `TotalAmount` da venda = soma dos `TotalPrice` dos itens - Discount da venda + Addition da venda
- Para finalizar, a venda deve ter pelo menos um item
- Ao finalizar, o estoque dos produtos é abatido
- Ao cancelar uma venda Finalizada, o estoque é revertido
- Ao cancelar uma venda Aberta, nenhuma alteração de estoque ocorre
- Não é possível excluir uma venda, apenas cancelar
- Não é possível editar uma venda Finalizada ou Cancelada
- O funcionário da venda é pré-preenchido com o funcionário do usuário logado

## Tela no Frontend (Avalonia)

### Navegação
- Menu: Movimentações → Vendas
- Abre em aba "Vendas" no TabControl principal

### Modo Lista
- DataGrid: Data, Funcionário, Valor Total, Situação
- Ações: Visualizar (olho), Editar (lápis — abre formulário se Aberta, ou visualização se não)
- Botão "+" (F1) cria nova venda

### Modo Formulário (Edição — Aberta)
- ComboBox de funcionário (pré-selecionado)
- Campos de desconto/acréscimo da venda
- DataGrid de itens com colunas: Referência, Nome, Qtd, Preço Un., Desconto, Acréscimo, Total
- Botões "Adicionar Item" e "Remover Item"
- Valor total em destaque
- Menu "Ações" com "Finalizar Venda" e "Cancelar Venda"

### Dialog de Adicionar Item
- Busca de produto via ProductSearchDialog (referência, nome, categoria, estoque)
- Campo de quantidade, desconto, acréscimo
- Validação de estoque antes de confirmar

### Modo Visualização (Finalizada/Cancelada)
- Campos desabilitados
- Ação "Cancelar Venda" disponível se Finalizada

## Relações

```
Sale ──── Employee     (N vendas → 1 funcionário)
Sale ──── SaleItem     (1 venda → N itens)
SaleItem ──── Product  (N itens → 1 produto)
```
