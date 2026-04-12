# Cadastro de Funcionários

## Entidade: Employee

| Campo | Tipo | Descrição |
|---|---|---|
| EmployeeId | Guid | PK |
| Name | string | Nome completo |
| Cpf | string | CPF (11 dígitos numéricos) |
| Role | string | Papel no sistema (`admin`, etc.) |
| CommissionRate | decimal | Taxa de comissão em % (ex: 5.00 = 5%) |
| CreatedAt | DateTime | Data de cadastro (UTC) |

## Endpoints

`GET/POST /api/employees` · `GET/PUT/DELETE /api/employees/{id}`

## Regras de negócio

- CPF: exatamente 11 dígitos numéricos; Nome obrigatório
- `CommissionRate` entre 0 e 100
- Um funcionário tem 0 ou 1 usuário de sistema vinculado
- Não é possível deletar funcionário com vendas registradas

## Relações

```
Employee → User       (0 ou 1 conta de acesso)
Employee → Sale       (N vendas)
Employee → Commission (N registros de comissão)
```
