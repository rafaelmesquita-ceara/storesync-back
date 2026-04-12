# Comissionamento

## Entidade: Commission

`CommissionId (Guid PK)`, `EmployeeId (FK)`, `Reference (ex: 001)`, `StartDate`, `EndDate`, `CommissionRate (snapshot)`, `TotalSales`, `CommissionValue`, `Observation?`, `CreatedAt`

## Endpoints

| Método | Rota |
|---|---|
| GET | `/api/commissions` |
| GET | `/api/commissions/{id}` |
| GET | `/api/commissions/calculate?employeeId=&startDate=&endDate=` |
| POST | `/api/commissions` |
| DELETE | `/api/commissions/{id}` |

## Cálculo

`CommissionValue = TotalSales × (CommissionRate / 100)` — considera vendas com status ≠ Cancelada.

## Regras

1. Sem sobreposição de período por funcionário → erro com referência do conflito
2. `StartDate` ≤ `EndDate`
3. Sem vendas no período → bloqueia criação
4. Sem edição após criação; só visualizar ou excluir

## Integração Financeiro

Ao confirmar, oferece gerar conta a pagar: ref `COM{Reference}`, valor = `CommissionValue`, status Aberto, tipo Pagar. Em seguida, oferece liquidação imediata.

## Frontend

Menu: `Movimentações → Comissões`. Grid: Referência, Funcionário, Período, Total Vendas, Taxa, Comissão. Ações: visualizar + excluir. Fluxo: preencher → Calcular (preview) → Confirmar.
