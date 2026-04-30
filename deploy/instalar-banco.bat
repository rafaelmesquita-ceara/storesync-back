@echo off
chcp 65001 >nul
echo ============================================
echo   StoreSync - Instalacao do Banco de Dados
echo ============================================
echo.
echo Pre-requisito: PostgreSQL 16 instalado e rodando.
echo.

set PGUSER=postgres
set /p PGPASSWORD=Senha do usuario postgres:

echo.
echo [1/3] Criando usuario 'store'...
psql -U postgres -c "DO $$ BEGIN IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'store') THEN CREATE ROLE store WITH LOGIN PASSWORD 'StoreSync@2026!'; END IF; END $$;"

if %ERRORLEVEL% neq 0 (
    echo ERRO: Falha ao criar usuario. Verifique se o PostgreSQL esta rodando e a senha esta correta.
    pause
    exit /b 1
)

echo.
echo [2/3] Criando banco 'storesync'...
psql -U postgres -c "SELECT 1 FROM pg_database WHERE datname = 'storesync'" | findstr /c:"1" >nul
if %ERRORLEVEL% neq 0 (
    psql -U postgres -c "CREATE DATABASE storesync OWNER store ENCODING 'UTF8' LC_COLLATE 'pt_BR.UTF-8' LC_CTYPE 'pt_BR.UTF-8' TEMPLATE template0;"
    if %ERRORLEVEL% neq 0 (
        echo Tentando criar sem locale especifico...
        psql -U postgres -c "CREATE DATABASE storesync OWNER store ENCODING 'UTF8';"
    )
) else (
    echo Banco 'storesync' ja existe, pulando criacao.
)

echo.
echo [3/3] Concedendo permissoes...
psql -U postgres -d storesync -c "GRANT ALL PRIVILEGES ON DATABASE storesync TO store;"
psql -U postgres -d storesync -c "GRANT ALL ON SCHEMA public TO store;"
psql -U postgres -d storesync -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO store;"
psql -U postgres -d storesync -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO store;"

echo.
echo ============================================
echo   Banco instalado com sucesso!
echo ============================================
echo.
echo   Banco: storesync
echo   Usuario: store
echo   Senha: StoreSync@2026!
echo   Porta: 5432
echo.
echo   As migrations serao aplicadas automaticamente
echo   na primeira execucao da API.
echo.
pause
