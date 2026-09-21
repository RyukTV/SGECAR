@echo off
title SGECAR - Detener Servicios
echo =======================================================
echo    Deteniendo servicios de SGECAR...
echo =======================================================
echo.

taskkill /F /FI "WINDOWTITLE eq SGECAR*" >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1

echo [OK] Servicios detenidos correctamente.
echo.
timeout /t 2 >nul
