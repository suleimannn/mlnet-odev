@echo off
chcp 65001 >nul
title ML.NET Odev
set "UYG=%~dp0uygulama"
if not exist "%UYG%\Web.exe" (
  echo "uygulama" klasoru bulunamadi. BASLAT.bat ile ayni klasorde olmali.
  pause
  exit /b
)
cd /d "%UYG%"
echo ML.NET ornekleri baslatiliyor, tarayici birazdan acilacak...
echo Kapatmak icin bu pencereyi kapatin.
"%UYG%\Web.exe"
pause
