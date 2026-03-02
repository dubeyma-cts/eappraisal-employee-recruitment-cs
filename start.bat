@echo off
title e-Appraisal System Launcher

echo ============================================
echo   e-Appraisal System - Starting Services
echo ============================================
echo.

cd /d "%~dp0src\Gateways\eAppraisal.Api"
echo [1/2] Starting API Server on http://localhost:5100 ...
start /B dotnet run --urls "http://localhost:5100" > nul 2>&1

echo [2/2] Starting Web App on http://localhost:5200 ...
cd /d "%~dp0src\Web\eAppraisal.Web"
start /B dotnet run --urls "http://localhost:5200" > nul 2>&1

echo.
echo Waiting for services to start...
timeout /t 15 /nobreak > nul

echo Opening browser...
start http://localhost:5200

echo.
echo ============================================
echo   API  : http://localhost:5100
echo   Web  : http://localhost:5200
echo ============================================
echo.
echo Press any key to stop all services and exit...
pause > nul

echo Stopping services...
taskkill /F /IM dotnet.exe > nul 2>&1
echo Done.
