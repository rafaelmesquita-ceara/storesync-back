# Funcionários

## Entidade: Employee

`EmployeeId (Guid PK)`, `Name`, `Cpf (11 dígitos)`, `Role (admin/…)`, `CommissionRate (0–100%)`, `CreatedAt`

## Endpoints

`GET/POST /api/employees` · `GET/PUT/DELETE /api/employees/{id}`

## Regras

- CPF: exatamente 11 dígitos numéricos; Name obrigatório; CommissionRate 0–100
- 0 ou 1 usuário de sistema vinculado
- Não deleta funcionário com vendas registradas

## Relações

`Employee → User (0..1)` · `Employee → Sale (N)` · `Employee → Commission (N)`
