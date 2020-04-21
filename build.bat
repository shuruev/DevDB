@ECHO OFF
SETLOCAL

IF EXIST "nuget\*" DEL "nuget\*" /Q

dotnet pack -c Release --output nuget

ENDLOCAL
PAUSE
