@echo off
chcp 65001 >nul
echo ============================================
echo   StoreSync - Iniciando Sistema
echo ============================================
echo.

set SCRIPT_DIR=%~dp0
set DIST_DIR=%SCRIPT_DIR%dist

:: Verifica se a publicacao existe
if not exist "%DIST_DIR%\api\StoreSyncBack.exe" (
    echo ERRO: API nao encontrada. Execute publicar.bat primeiro.
    pause
    exit /b 1
)

if not exist "%DIST_DIR%\front\StoreSyncFront.exe" (
    echo ERRO: Frontend nao encontrado. Execute publicar.bat primeiro.
    pause
    exit /b 1
)

:: Inicia a API em background
echo Iniciando API na porta 5269...
start "StoreSync API" /min "%DIST_DIR%\api\StoreSyncBack.exe" --urls "http://localhost:5269" --environment Production

:: Aguarda API subir
echo Aguardando API inicializar...
timeout /t 5 /nobreak >nul

:: Inicia o Frontend
echo Iniciando Frontend...
start "StoreSync" "%DIST_DIR%\front\StoreSyncFront.exe"

echo.
echo ============================================
echo   Sistema iniciado!
echo ============================================
echo.
echo   API:   http://localhost:5269
echo   Login: admin / admin
echo.
echo   Para parar a API, feche a janela minimizada
echo   "StoreSync API" na barra de tarefas.
echo.
