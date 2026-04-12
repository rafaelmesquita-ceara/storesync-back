# Financeiro — Contas a Pagar/Receber

## Entidade: Finance

`FinanceId (Guid PK)`, `Reference`, `Description`, `Amount (>0)`, `DueDate`, `Status (1=Aberto 2=Liquidado 3=LiquidadoParcialmente)`, `Type (1=Pagar 2=Receber)`, `TitleType (1=Original 2=Residual)`, `SettledAmount?`, `SettledAt?`, `SettledNote?`, `ParentId? (FK título original)`, `CreatedAt`

## Endpoints

| Método | Rota |
|---|---|
| GET | `/api/Finance` / `/api/Finance?type={1\|2}` |
| GET | `/api/Finance/{id}` |
| POST | `/api/Finance` |
| PUT | `/api/Finance/{id}` |
| DELETE | `/api/Finance/{id}` — só Status=Aberto |
| POST | `/api/Finance/{id}/settle` — `{ settledAmount, note }` |
| DELETE | `/api/Finance/{id}/settle` — cancela liquidação |

## Regras

- Só exclui títulos Abertos
- `SettledAmount == Amount` → Status 2 (Liquidado)
- `SettledAmount < Amount` → Status 3 + gera Residual com valor restante
- Cancelar liquidação de status 3: só se Residual associado já foi excluído

## Frontend

Telas "Contas a Pagar" e "Contas a Receber" compartilham `FinancesView`/`FinancesViewModel` parametrizados por `FinanceType`. Botão **Ações** expande dropdown com Liquidar/Cancelar Liquidação. Paginação: 50/página com `CurrentPage`, `TotalPages`, `CanPreviousPage`, `CanNextPage`.
