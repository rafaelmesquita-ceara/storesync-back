# Controle de Acesso

Auth via JWT. Única rota pública: login. Todas as demais exigem `Authorization: Bearer <token>`.

## Entidades

**User** — conta de acesso, sempre vinculada a um `Employee`.

| Campo | Tipo | Descrição |
|---|---|---|
| UserId | Guid | PK |
| Login | string | Nome de usuário (único) |
| Password | string | Senha hasheada (BCrypt) |
| EmployeeId | Guid | Funcionário vinculado |

**Role** (definida no Employee):

| Role | Acesso |
|---|---|
| `admin` | Acesso total |

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/users/login` | Público | Autentica, retorna token JWT |
| POST | `/api/users` | Admin | Cria usuário |
| GET | `/api/users` | Autenticado | Lista usuários |
| GET | `/api/users/{id}` | Autenticado | Busca por ID |
| PUT | `/api/users/{id}` | Admin | Atualiza usuário |
| DELETE | `/api/users/{id}` | Admin | Remove usuário |
| POST | `/api/users/change-password` | Autenticado | Altera senha |

## Regras de negócio

- Senha nunca retornada nas respostas (campo omitido)
- Login duplicado não permitido
- Token JWT contém: `UserId`, `Role`, expiração
- Usuário só altera a própria senha com confirmação da senha atual
