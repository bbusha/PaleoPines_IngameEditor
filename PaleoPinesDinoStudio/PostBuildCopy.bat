@echo off
setlocal

REM --- File to copy ---
set "SOURCE=C:\Users\chris\source\repos\PaleoPines_IngameEditor\PaleoPinesDinoStudio\bin\PaleoPinesDinoStudio.dll"

REM --- Target directories ---
set "STEAMDIR=C:\Program Files (x86)\Steam\steamapps\common\Paleo Pines\mods"
set "GAMEDIR=C:\Games\Paleo Pines\mods"

REM --- Ensure source exists ---
if not exist "%SOURCE%" (
    echo ERROR: Source file not found:
    echo   %SOURCE%
    exit /b 1
)

REM --- Check Steam directory ---
if exist "%STEAMDIR%" (
    echo Found Steam mods folder.
    copy /Y "%SOURCE%" "%STEAMDIR%"
    echo Copied to Steam mods folder.
    exit /b 0
)

REM --- Check C:\Games directory ---
if exist "%GAMEDIR%" (
    echo Found C:\Games mods folder.
    copy /Y "%SOURCE%" "%GAMEDIR%"
    echo Copied to C:\Games mods folder.
    exit /b 0
)

echo ERROR: No Paleo Pines mods folder found.
echo Checked:
echo   "%STEAMDIR%"
echo   "%GAMEDIR%"

exit /b 1
