# Cadastro de Funcionários

## Objetivo

Registrar e gerenciar os funcionários da loja. Cada funcionário pode ter uma conta de acesso ao sistema e uma taxa de comissão sobre suas vendas.

## Entidade: Employee

| Campo | Tipo | Descrição |
|---|---|---|
| EmployeeId | Guid | Identificador único |
| Name | string | Nome completo |
| Cpf | string | CPF (11 dígitos, sem formatação) |
| Role | string | Papel no sistema (`admin`, etc.) |
| CommissionRate | decimal | Taxa de comissão em % (ex: 5.00 = 5%) |
| CreatedAt | DateTime | Data de cadastro (UTC) |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/employees` | Lista todos os funcionários |
| GET | `/api/employees/{id}` | Busca funcionário por ID |
| POST | `/api/employees` | Cadastra novo funcionário |
| PUT | `/api/employees/{id}` | Atualiza dados do funcionário |
| DELETE | `/api/employees/{id}` | Remove funcionário |

## Regras de negócio

- CPF deve ter exatamente 11 dígitos numéricos
- Nome é obrigatório
- `CommissionRate` deve ser entre 0 e 100
- Um funcionário pode ter zero ou um usuário de sistema vinculado
- Não é possível deletar um funcionário que tenha vendas registradas

## Relações

```
Employee ──── User          (1 funcionário → 0 ou 1 conta de acesso)
Employee ──── Sale          (1 funcionário → N vendas)
Employee ──── Commission    (1 funcionário → N registros de comissão)
```
