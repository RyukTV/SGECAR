@echo off
title SGECAR - Detener Servicios
echo =======================================================
echo    Deteniendo servicios de SGECAR...
echo =======================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-NetTCPConnection -LocalPort 5080, 5180 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }; Get-Process dotnet, SistemaGestionEmpresarial.Api, SistemaGestionEmpresarial.Web -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue; Get-Process cmd -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like '*SGECAR*' } | Stop-Process -Force -ErrorAction SilentlyContinue"

echo [OK] Servicios detenidos y puertos 5080 y 5180 liberados correctamente.
echo.
ping 127.0.0.1 -n 2 >nul
