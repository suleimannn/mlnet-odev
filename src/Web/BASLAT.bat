@echo off
chcp 65001 >nul
title ML.NET Odev
cd /d "%~dp0"
echo ML.NET ornekleri baslatiliyor, tarayici birazdan acilacak...
echo Kapatmak icin bu pencereyi kapatin.
Web.exe
pause
