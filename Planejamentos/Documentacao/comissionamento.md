# Comissionamento

## Entidade: Commission

| Campo | Tipo | Descrição |
|---|---|---|
| CommissionId | Guid | PK |
| EmployeeId | Guid | Funcionário comissionado |
| Reference | string | Código de identificação (ex: 001) |
| StartDate | DateTime | Data inicial do período |
| EndDate | DateTime | Data final do período |
| CommissionRate | decimal | Snapshot da taxa no momento da criação |
| TotalSales | decimal | Total de vendas não canceladas do período |
| CommissionValue | decimal | Valor da comissão calculada |
| Observation | string? | Observação livre |
| CreatedAt | DateTime | Data de criação |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/commissions` | Lista todas as comissões |
| GET | `/api/commissions/{id}` | Busca por ID |
| GET | `/api/commissions/calculate?employeeId=&startDate=&endDate=` | Calcula preview sem salvar |
| POST | `/api/commissions` | Cria nova comissão |
| DELETE | `/api/commissions/{id}` | Exclui comissão |

## Cálculo

`CommissionValue = TotalSales × (CommissionRate / 100)`

- `TotalSales` considera apenas vendas com status ≠ Cancelada (Aberta + Finalizada)
- `CommissionRate` é snapshoteada do `Employee.CommissionRate` no momento da criação

## Regras de Negócio

1. **Sobreposição de período**: Não é permitido criar comissionamento para um funcionário em um período que se sobreponha a outro já existente. Erro: `"Já existe um comissionamento para este funcionário no período informado. Referência: {ref}"`
2. **Datas**: `StartDate` não pode ser maior que `EndDate`. Erro: `"Data inicial não pode ser maior que a data final."`
3. **Sem vendas**: Se não houver vendas no período para o funcionário, a criação é bloqueada. Erro: `"Nenhuma venda encontrada para o funcionário no período informado."`
4. **Sem edição**: Não é possível editar um comissionamento após a criação; apenas visualizar ou excluir.

## Integração com Financeiro

Ao confirmar um comissionamento, o sistema pergunta se deve gerar uma conta a pagar:
- **Referência**: `COM{Reference}` (ex: COM001)
- **Descrição**: `"Conta a pagar gerada automaticamente pela rotina de comissionamento"`
- **Valor**: igual ao `CommissionValue`
- **Status inicial**: Aberto
- **Tipo**: Pagar

Em seguida, o sistema pergunta se deseja dar baixa imediatamente (liquidação total).

## Tela (Frontend)

- Menu: `_Movimentações → Comissões`
- Grid: Referência, Funcionário, Período, Total Vendas, Taxa, Comissão
- Ações por linha: Visualizar (olho) + Excluir (lixeira)
- Fluxo de criação: preencher campos → Calcular (preview) → Confirmar (salvar + fluxo financeiro)
