# Controle de Acesso

Auth via JWT. Única rota pública: login. Todas as demais exigem `Authorization: Bearer <token>`.

## Entidades

**User:** `UserId (Guid PK)`, `Login (único)`, `Password (BCrypt)`, `EmployeeId (FK)`

**Role** (definida no Employee): `admin` = acesso total

## Endpoints

| Método | Rota | Auth |
|---|---|---|
| POST | `/api/users/login` | Público |
| POST | `/api/users` | Admin |
| GET | `/api/users` | Autenticado |
| GET | `/api/users/{id}` | Autenticado |
| PUT | `/api/users/{id}` | Admin |
| DELETE | `/api/users/{id}` | Admin |
| POST | `/api/users/change-password` | Autenticado |

## Regras

- Senha nunca retornada; login duplicado → 409
- JWT contém: `UserId`, `Role`, expiração
- Usuário só altera própria senha com confirmação da senha atual
