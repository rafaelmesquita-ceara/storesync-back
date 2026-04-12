# Controle Financeiro — Contas a Pagar / Receber

## Entidade: Finance

| Campo | Tipo | Descrição |
|---|---|---|
| FinanceId | Guid | PK |
| Reference | string | Referência/código visual |
| Description | string | Descrição do lançamento |
| Amount | decimal | Valor do título |
| DueDate | DateTime | Data de vencimento |
| Status | int | 1=Aberto, 2=Liquidado, 3=LiquidadoParcialmente |
| Type | int | 1=Pagar, 2=Receber |
| TitleType | int | 1=Original, 2=Residual |
| SettledAmount | decimal? | Valor liquidado |
| SettledAt | DateTime? | Data da liquidação |
| SettledNote | string? | Observação da liquidação |
| ParentId | Guid? | FK para o título original (só em residuais) |
| CreatedAt | DateTime | Data de criação |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/Finance` | Lista todos os títulos |
| GET | `/api/Finance?type={1\|2}` | Filtra por tipo |
| GET | `/api/Finance/{id}` | Busca por ID |
| POST | `/api/Finance` | Cria título |
| PUT | `/api/Finance/{id}` | Atualiza título |
| DELETE | `/api/Finance/{id}` | Remove (apenas Status=Aberto) |
| POST | `/api/Finance/{id}/settle` | Liquida título `{ settledAmount, note }` |
| DELETE | `/api/Finance/{id}/settle` | Cancela liquidação |

## Regras de negócio

- `Amount` > 0; `Description` e `DueDate` obrigatórios; Status padrão: `1` (Aberto)
- Apenas títulos **Abertos** podem ser excluídos
- `SettledAmount` ≤ `Amount`
- `SettledAmount == Amount` → Status `2` (Liquidado)
- `SettledAmount < Amount` → Status `3` (LiquidadoParcialmente) + gera título **Residual** com o valor restante
- Cancelar liquidação de status `3` só é permitido se o título **Residual** associado já foi excluído

## Frontend (Avalonia)

Telas "Contas a Pagar" e "Contas a Receber" compartilham `FinancesView` + `FinancesViewModel`, parametrizados por `FinanceType`. Botão **Ações** expande dropdown com **Liquidar** / **Cancelar Liquidação** conforme o status atual.
