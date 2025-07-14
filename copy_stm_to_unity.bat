@echo off
setlocal

REM === Konfiguration ===
set PYTHON_DIR=python_runtime
set SCRIPT_DIR=PythonScripts
set CSHARP_BUILD_DIR=CSharpHost\out

set OUT_DIR=UnityImplementation\Assets\ScanToMesh
set CS_OUT_DIR=%OUT_DIR%\CSharpHost
set PY_OUT_DIR=%OUT_DIR%\python_runtime
set SCRIPTS_OUT_DIR=%OUT_DIR%\PythonScripts


REM === OUT-Zielordner erstellen ===
echo Erstelle Zielordner...
if not exist "%CS_OUT_DIR%" mkdir "%CS_OUT_DIR%"
if not exist "%PY_OUT_DIR%" mkdir "%PY_OUT_DIR%"
if not exist "%SCRIPTS_OUT_DIR%" mkdir "%SCRIPTS_OUT_DIR%"


REM === Python-Runtime kopieren ===
if exist "%PYTHON_DIR%" (
    echo Kopiere %PYTHON_DIR% nach %PY_OUT_DIR% ...
    xcopy /E /I /Y "%PYTHON_DIR%" "%PY_OUT_DIR%"
) else (
    echo [WARNUNG] %PYTHON_DIR% nicht gefunden.
)

REM === Python-Skripte kopieren ===
if exist "%SCRIPT_DIR%" (
    echo Kopiere %SCRIPT_DIR% nach %SCRIPTS_OUT_DIR% ...
    xcopy /E /I /Y "%SCRIPT_DIR%" "%SCRIPTS_OUT_DIR%"
) else (
    echo [WARNUNG] %SCRIPT_DIR% nicht gefunden.
)

REM === C# Build Output kopieren ===
if exist "%CSHARP_BUILD_DIR%" (
    echo Kopiere %CSHARP_BUILD_DIR% nach %CS_OUT_DIR% ...
    xcopy /E /I /Y "%CSHARP_BUILD_DIR%" "%CS_OUT_DIR%"
) else (
    echo [WARNUNG] %CSHARP_BUILD_DIR% nicht gefunden.
)


echo.
echo === Kopiervorgang abgeschlossen ===
pause
endlocal