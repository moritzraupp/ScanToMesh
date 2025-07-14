@echo off
SETLOCAL

SET PYTHON=python_runtime\python.exe

echo Installing Python dependencies...
%PYTHON% -m pip install -r python_runtime\requirements.txt

echo Done!
pause
ENDLOCAL