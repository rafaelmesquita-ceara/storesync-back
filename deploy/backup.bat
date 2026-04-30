@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
:: ============================================
::   StoreSync - Backup Automatico do Banco
:: ============================================
::
:: Este script faz backup do banco storesync.
:: Pode ser executado manualmente ou agendado
:: via Agendador de Tarefas do Windows.
::
:: Retencao: mantem os ultimos 30 backups.

set PGPASSWORD=StoreSync@2026!
set BACKUP_DIR=%~dp0backups
set DB_NAME=storesync
set DB_USER=store
set DB_HOST=localhost
set DB_PORT=5432
set RETENTION=30

:: Cria pasta de backups se nao existir
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

:: Gera nome do arquivo com data/hora
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /format:list') do set datetime=%%I
set TIMESTAMP=%datetime:~0,4%-%datetime:~4,2%-%datetime:~6,2%_%datetime:~8,2%h%datetime:~10,2%m

set BACKUP_FILE=%BACKUP_DIR%\storesync_%TIMESTAMP%.backup

:: Executa o backup
pg_dump -U %DB_USER% -h %DB_HOST% -p %DB_PORT% -d %DB_NAME% -F c -f "%BACKUP_FILE%"

if %ERRORLEVEL% neq 0 (
    echo [%date% %time%] ERRO: Falha no backup >> "%BACKUP_DIR%\backup.log"
    exit /b 1
)

:: Log de sucesso
echo [%date% %time%] Backup OK: %BACKUP_FILE% >> "%BACKUP_DIR%\backup.log"

:: Remove backups antigos (mantem os ultimos N)
set count=0
for /f "delims=" %%f in ('dir /b /o-d "%BACKUP_DIR%\storesync_*.backup" 2^>nul') do (
    set /a count+=1
    if !count! gtr %RETENTION% del "%BACKUP_DIR%\%%f"
)

exit /b 0
