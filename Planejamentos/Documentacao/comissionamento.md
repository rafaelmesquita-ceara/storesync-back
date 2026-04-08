# Comissionamento

## Objetivo

Calcular e registrar automaticamente a comissão de cada funcionário com base nas vendas realizadas em um determinado período.

## Entidade: Commission

| Campo | Tipo | Descrição |
|---|---|---|
| CommissionId | Guid | Identificador único |
| EmployeeId | Guid | Funcionário comissionado |
| Month | DateTime | Mês/ano de referência (dia sempre = 1) |
| TotalSales | decimal | Valor total de vendas do período |
| CommissionValue | decimal | Valor da comissão calculada |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/commissions` | Lista todos os registros de comissão |
| GET | `/api/commissions/{id}` | Busca comissão por ID |
| POST | `/api/commissions` | Registra comissão manualmente |
| PUT | `/api/commissions/{id}` | Atualiza registro de comissão |
| DELETE | `/api/commissions/{id}` | Remove registro |

## Cálculo

```
CommissionValue = TotalSales × (Employee.CommissionRate / 100)
```

**Exemplo:** funcionário com `CommissionRate = 5%` e `TotalSales = R$ 10.000`
→ `CommissionValue = R$ 500,00`

## Regras de negócio

- `CommissionRate` é definida no cadastro do funcionário e pode variar por pessoa
- O período de referência é mensal
- Um funcionário pode ter no máximo um registro de comissão por mês
- `TotalSales` considera apenas vendas do mês de referência vinculadas ao funcionário
- `CommissionValue` nunca pode ser negativo

## Relações

```
Commission ──── Employee   (N comissões → 1 funcionário)
```

> **Evolução futura:** cálculo automático ao fechar o mês (job agendado), relatório de comissões por período, exportação para folha de pagamento.
