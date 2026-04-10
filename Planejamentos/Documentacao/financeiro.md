# Controle Financeiro — Contas a Pagar / Contas a Receber

## Objetivo

Registrar e controlar títulos a pagar e a receber, com suporte a liquidação total, liquidação parcial (gerando um título residual) e cancelamento de liquidação.

## Entidade: Finance

| Campo | Tipo | Descrição |
|---|---|---|
| FinanceId | Guid | Identificador único |
| Reference | string | Referência/código para identificação visual |
| Description | string | Descrição do lançamento |
| Amount | decimal | Valor do título |
| DueDate | DateTime | Data de vencimento |
| Status | int | Situação do título (ver tabela abaixo) |
| Type | int | Tipo: Pagar ou Receber |
| TitleType | int | Natureza: Original ou Residual |
| SettledAmount | decimal? | Valor liquidado |
| SettledAt | DateTime? | Data da liquidação |
| SettledNote | string? | Observação da liquidação |
| ParentId | Guid? | FK para o título original (apenas em residuais) |
| CreatedAt | DateTime | Data de criação |

### Status (int)

| Valor | Constante | Descrição |
|---|---|---|
| `1` | `FinanceStatus.Aberto` | Título em aberto — não liquidado |
| `2` | `FinanceStatus.Liquidado` | Liquidado integralmente |
| `3` | `FinanceStatus.LiquidadoParcialmente` | Liquidado parcialmente — existe um residual |

### Tipo (int)

| Valor | Constante | Descrição |
|---|---|---|
| `1` | `FinanceType.Pagar` | Conta a pagar |
| `2` | `FinanceType.Receber` | Conta a receber |

### Tipo do Título (int)

| Valor | Constante | Descrição |
|---|---|---|
| `1` | `FinanceTitleType.Original` | Título original |
| `2` | `FinanceTitleType.Residual` | Título residual gerado por liquidação parcial |

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/Finance` | Lista todos os títulos |
| GET | `/api/Finance?type={1\|2}` | Lista filtrado por tipo (Pagar/Receber) |
| GET | `/api/Finance/{id}` | Busca título por ID |
| POST | `/api/Finance` | Cria novo título |
| PUT | `/api/Finance/{id}` | Atualiza título |
| DELETE | `/api/Finance/{id}` | Remove título (apenas se Status = Aberto) |
| POST | `/api/Finance/{id}/settle` | Liquida um título |
| DELETE | `/api/Finance/{id}/settle` | Cancela a liquidação de um título |

### Body de `POST /api/Finance/{id}/settle`

```json
{
  "settledAmount": 250.00,
  "note": "Pago via transferência"
}
```

## Regras de negócio

- `Amount` deve ser maior que zero
- `Description` é obrigatória
- `DueDate` é obrigatória
- Status padrão ao criar: `1` (Aberto)
- Apenas títulos em **Aberto** (`Status = 1`) podem ser excluídos
- `SettledAmount` não pode ser maior que `Amount`
- Se `SettledAmount == Amount` → Status vai para **Liquidado** (`2`)
- Se `SettledAmount < Amount` → Status vai para **Liquidado Parcialmente** (`3`) e é gerado um novo título **Residual** com o valor restante
- Cancelar liquidação de um título **Liquidado Parcialmente** (`3`) só é permitido se o título **Residual** associado já tiver sido excluído; caso contrário, o sistema orienta o usuário a excluí-lo primeiro

## Telas (Frontend Avalonia)

As telas "Contas a Pagar" e "Contas a Receber" compartilham a mesma view `FinancesView` e `FinancesViewModel`, parametrizados pelo tipo (`FinanceType.Pagar` ou `FinanceType.Receber`).

### Modo Lista
- DataGrid com colunas: Referência, Descrição, Vencimento, Valor, Tipo (Original/Residual), Situação
- Botão **+** (F1) para novo registro
- Botão Editar e Excluir por linha

### Modo Edição
- Formulário: Referência, Descrição, Valor, Vencimento
- Botão **Ações** (expande dropdown com as ações disponíveis):
  - **Liquidar** — habilitado se Status = Aberto ou Liquidado Parcialmente
  - **Cancelar Liquidação** — habilitado se Status = Liquidado ou Liquidado Parcialmente
- Ao clicar em **Liquidar**: abre dialog pedindo Valor a liquidar e Observação
