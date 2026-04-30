@echo off
chcp 65001 >nul
echo ============================================
echo   StoreSync - Publicacao para Piloto Local
echo ============================================
echo.

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set OUTPUT_DIR=%SCRIPT_DIR%dist

:: Limpa publicacao anterior
if exist "%OUTPUT_DIR%" (
    echo Removendo publicacao anterior...
    rmdir /s /q "%OUTPUT_DIR%"
)

mkdir "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%\api"
mkdir "%OUTPUT_DIR%\front"

echo.
echo [1/2] Publicando API (backend)...
echo.
dotnet publish "%ROOT_DIR%\StoreSyncBack\StoreSyncBack.csproj" ^
    -c Release ^
    -r win-x64 ^
    --self-contained ^
    -o "%OUTPUT_DIR%\api" ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERRO: Falha ao publicar a API.
    pause
    exit /b 1
)

:: Copia appsettings de producao
copy "%ROOT_DIR%\StoreSyncBack\appsettings.Production.json" "%OUTPUT_DIR%\api\appsettings.Production.json" >nul

echo.
echo [2/2] Publicando Frontend (Avalonia)...
echo.
dotnet publish "%ROOT_DIR%\StoreSyncFront\StoreSyncFront.csproj" ^
    -c Release ^
    -r win-x64 ^
    --self-contained ^
    -o "%OUTPUT_DIR%\front" ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true

if %ERRORLEVEL% neq 0 (
    echo.
    echo ERRO: Falha ao publicar o Frontend.
    pause
    exit /b 1
)

echo.
echo ============================================
echo   Publicacao concluida com sucesso!
echo   Saida: %OUTPUT_DIR%
echo ============================================
echo.
echo   api\    - Backend (StoreSyncBack.exe)
echo   front\  - Frontend (StoreSyncFront.exe)
echo.
pause
