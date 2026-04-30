# StoreSync - Guia de Deploy Local (Piloto)

## Pre-requisitos na maquina do cliente

1. **PostgreSQL 16** - https://www.postgresql.org/download/windows/
   - Durante instalacao: anotar a senha do usuario `postgres`
   - Marcar opcao de adicionar ao PATH
   - Porta padrao: 5432

## Passo a passo

### 1. Publicar o sistema (na maquina de desenvolvimento)

```
deploy\publicar.bat
```

Gera a pasta `deploy\dist\` com os executaveis standalone (API + Front).
Copiar a pasta `deploy\` inteira para a maquina do cliente.

### 2. Instalar o banco (na maquina do cliente)

```
deploy\instalar-banco.bat
```

- Pede a senha do usuario `postgres` (definida na instalacao do PostgreSQL)
- Cria o usuario `store` e o banco `storesync`
- As migrations sao aplicadas automaticamente na primeira execucao da API

### 3. Iniciar o sistema

```
deploy\iniciar.bat
```

- Sobe a API minimizada na porta 5269
- Abre o frontend automaticamente
- Login inicial: **admin / admin**

### 4. Agendar backup automatico (executar como Administrador)

```
deploy\agendar-backup.bat
```

- Cria tarefa no Agendador do Windows
- Backup diario as 22:00
- Mantem os ultimos 30 backups em `deploy\backups\`
- Log em `deploy\backups\backup.log`

### 5. Parar o sistema

```
deploy\parar.bat
```

## Restaurar backup

Caso precise restaurar um backup:

```
pg_restore -U store -h localhost -d storesync -c "deploy\backups\storesync_AAAA-MM-DD_HHhMMm.backup"
```

## Estrutura da pasta deploy

```
deploy\
  publicar.bat          - Gera os executaveis
  instalar-banco.bat    - Cria banco e usuario no PostgreSQL
  agendar-backup.bat    - Agenda backup diario (admin)
  backup.bat            - Executa backup (manual ou agendado)
  iniciar.bat           - Sobe API + Frontend
  parar.bat             - Para tudo
  GUIA-DEPLOY.md        - Este guia
  dist\                 - Executaveis publicados
    api\                - StoreSyncBack.exe
    front\              - StoreSyncFront.exe
  backups\              - Backups do banco
```

## Credenciais

| Recurso | Usuario | Senha |
|---------|---------|-------|
| Banco (app) | store | StoreSync@2026! |
| Sistema | admin | admin |

**Importante:** Trocar a senha do usuario `admin` no primeiro acesso.

## Notas

- Token JWT expira em 8 horas (jornada de trabalho)
- Swagger desabilitado em producao
- Logs em nivel Warning (menos verbose)
- Sem necessidade de .NET SDK nem Docker na maquina do cliente
