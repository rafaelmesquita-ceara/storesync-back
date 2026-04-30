@echo off
chcp 65001 >nul
echo Parando StoreSync...
taskkill /im StoreSyncBack.exe /f 2>nul
taskkill /im StoreSyncFront.exe /f 2>nul
echo Sistema parado.
timeout /t 2 /nobreak >nul
