@echo off
setlocal
REM Opens the GHETTO BEASTS project folder, or launches Godot if GODOT is set.
REM Example: setx GODOT "C:\Program Files\Godot\Godot_v4.3-stable_mono_win64.exe"

cd /d "%~dp0.."

if defined GODOT (
  if exist "%GODOT%" (
    echo Starting Godot: %GODOT%
    start "" "%GODOT%" --path "%CD%"
    exit /b 0
  )
  echo GODOT is set but file not found: %GODOT%
)

echo.
echo === GHETTO BEASTS – in Godot oeffnen ===
echo.
echo 1. Installiere Godot 4.3+ .NET:
echo    https://godotengine.org/download/windows/
echo 2. Installiere .NET 8 SDK:
echo    https://dotnet.microsoft.com/download/dotnet/8.0
echo 3. Godot starten -^> Import/Open -^> diese Datei waehlen:
echo    %CD%\project.godot
echo 4. Build ^(Hammer^) klicken, dann F5.
echo 5. Tasten 1-5: Karten/Aktionen im Setup und Kampf.
echo.
echo Optional: setx GODOT "C:\Pfad\zu\Godot_v4.3-stable_mono_win64.exe"
echo           Dann startet dieses Skript Godot automatisch.
echo.
echo Projektordner wird geoeffnet...
start "" explorer "%CD%"
echo Siehe auch LAUNCH.md im Projektroot.
pause
endlocal
