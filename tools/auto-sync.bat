@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0auto-sync.ps1" %*
