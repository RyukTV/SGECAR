@echo off
setlocal
pushd "%~dp0"
title SGECAR - Iniciador Automatico
echo =======================================================
echo    SISTEMA DE GESTION EMPRESARIAL (SGECAR)
echo =======================================================
echo.

echo [1/3] Configurando variables de entorno...
set "PATH=%USERPROFILE%\.dotnet;%PATH%"

echo [2/3] Levantando Backend API (http://localhost:5080)...
start "SGECAR - Backend API" cmd /k "title SGECAR - API && set PATH=%USERPROFILE%\.dotnet;%%PATH%% && dotnet run --project src\SistemaGestionEmpresarial.Api --launch-profile http"

ping 127.0.0.1 -n 4 >nul

echo [3/3] Levantando Frontend Blazor (http://localhost:5180)...
start "SGECAR - Frontend Blazor" cmd /k "title SGECAR - Blazor Web && set PATH=%USERPROFILE%\.dotnet;%%PATH%% && dotnet run --project src\SistemaGestionEmpresarial.Web --launch-profile http"

ping 127.0.0.1 -n 4 >nul

echo.
echo =======================================================
echo  Todo listo! Abriendo navegador en http://localhost:5180...
echo =======================================================
start http://localhost:5180

echo.
echo Para detener los servicios ejecuta "detener.bat" o cierra las ventanas.
ping 127.0.0.1 -n 4 >nul
popd
