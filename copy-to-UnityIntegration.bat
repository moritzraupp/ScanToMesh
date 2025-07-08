@echo off
setlocal

REM === Konfiguration ===
set PYTHON_DIR=python_runtime
set SCRIPT_DIR=PythonScripts
set CSHARP_BUILD_DIR=CSharpHost\out
set UNITY_DIR=UnityIntegration\ScanToMesh
set UNITY_DLL_TARGET=%UNITY_DIR%\CSharpHost

REM === Unity-Zielordner erstellen ===
echo Erstelle Unity-Integrationsordner...
if not exist "%UNITY_DIR%" (
    mkdir "%UNITY_DIR%"
)

REM === Unterordner für DLLs erstellen ===
if not exist "%UNITY_DLL_TARGET%" (
    mkdir "%UNITY_DLL_TARGET%"
)

REM === Python-Runtime kopieren ===
if exist "%PYTHON_DIR%" (
    echo Kopiere %PYTHON_DIR% nach %UNITY_DIR%\python_runtime ...
    xcopy /E /I /Y "%PYTHON_DIR%" "%UNITY_DIR%\python_runtime"
) else (
    echo [WARNUNG] %PYTHON_DIR% nicht gefunden.
)

REM === Python-Skripte kopieren ===
if exist "%SCRIPT_DIR%" (
    echo Kopiere %SCRIPT_DIR% nach %UNITY_DIR%\PythonScripts ...
    xcopy /E /I /Y "%SCRIPT_DIR%" "%UNITY_DIR%\PythonScripts"
) else (
    echo [WARNUNG] %SCRIPT_DIR% nicht gefunden.
)

REM === C# Build Output kopieren ===
if exist "%CSHARP_BUILD_DIR%" (
    echo Kopiere %CSHARP_BUILD_DIR% nach %UNITY_DLL_TARGET% ...
    xcopy /E /I /Y "%CSHARP_BUILD_DIR%" "%UNITY_DLL_TARGET%"
) else (
    echo [WARNUNG] %CSHARP_BUILD_DIR% nicht gefunden.
)

echo.
echo === Kopiervorgang abgeschlossen ===
pause
endlocal