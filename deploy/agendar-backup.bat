@echo off
chcp 65001 >nul
echo ============================================
echo   StoreSync - Agendar Backup Automatico
echo ============================================
echo.
echo Este script cria uma tarefa agendada no Windows
echo para executar o backup diariamente as 22:00.
echo.
echo IMPORTANTE: Execute como Administrador!
echo.

set SCRIPT_DIR=%~dp0

:: Cria tarefa agendada - backup diario as 22:00
schtasks /create /tn "StoreSync Backup Diario" ^
    /tr "\"%SCRIPT_DIR%backup.bat\"" ^
    /sc daily ^
    /st 22:00 ^
    /ru SYSTEM ^
    /rl HIGHEST ^
    /f

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERRO: Falha ao criar tarefa agendada.
    echo Verifique se esta executando como Administrador.
    pause
    exit /b 1
)

echo.
echo ============================================
echo   Backup agendado com sucesso!
echo ============================================
echo.
echo   Tarefa: StoreSync Backup Diario
echo   Horario: Todos os dias as 22:00
echo   Destino: %SCRIPT_DIR%backups\
echo   Retencao: 30 backups
echo.
echo   Para verificar: Agendador de Tarefas do Windows
echo   Para remover:  schtasks /delete /tn "StoreSync Backup Diario" /f
echo.
pause
