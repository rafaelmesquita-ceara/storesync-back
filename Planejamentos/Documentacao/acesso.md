# Controle de Acesso

## Objetivo

Garantir que apenas usuários autenticados acessem o sistema, e que cada perfil tenha acesso apenas ao que lhe é permitido.

## Como funciona

O acesso é baseado em **JWT (JSON Web Token)**. Ao fazer login, o usuário recebe um token que deve ser enviado no header `Authorization: Bearer <token>` em todas as requisições subsequentes.

Todas as rotas da API são protegidas por padrão. A única rota pública é o login.

## Entidades envolvidas

### User
Representa a conta de acesso ao sistema. Está sempre vinculado a um `Employee`.

| Campo | Tipo | Descrição |
|---|---|---|
| UserId | Guid | Identificador único |
| Login | string | Nome de usuário |
| Password | string | Senha hasheada (BCrypt) |
| EmployeeId | Guid | Funcionário vinculado |

### Employee (papel/role)
O nível de acesso é definido pelo campo `Role` do funcionário vinculado ao usuário.

| Role | Acesso |
|---|---|
| `admin` | Acesso total ao sistema |
| *(outros)* | A definir conforme necessidade |

## Fluxo de autenticação

```
1. POST /api/users/login  { login, password }
        │
        ▼
2. Sistema valida credenciais e compara senha com hash BCrypt
        │
        ▼
3. Se válido → gera token JWT com claims (UserId, Role)
        │
        ▼
4. Cliente envia token no header de todas as próximas requisições
        │
        ▼
5. Middleware valida o token a cada requisição
```

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/users/login` | Público | Autentica e retorna token JWT |
| POST | `/api/users` | Admin | Cria novo usuário |
| GET | `/api/users` | Autenticado | Lista usuários |
| GET | `/api/users/{id}` | Autenticado | Busca usuário por ID |
| PUT | `/api/users/{id}` | Admin | Atualiza usuário |
| DELETE | `/api/users/{id}` | Admin | Remove usuário |
| POST | `/api/users/change-password` | Autenticado | Altera senha |

## Regras de negócio

- Senha nunca é retornada nas respostas da API (campo omitido)
- Senha é hasheada com BCrypt antes de ser salva
- Token JWT contém: `UserId`, `Role`, tempo de expiração
- Login duplicado não é permitido
- Usuário só pode alterar a própria senha mediante confirmação da senha atual

## Configuração JWT (appsettings.json)

```json
"Jwt": {
  "Key": "...",
  "Issuer": "StoreSyncBack",
  "Audience": "StoreSyncFront",
  "ExpiresInMinutes": 60
}
```
