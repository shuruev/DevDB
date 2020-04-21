@ECHO OFF
SETLOCAL

SET nuget=%UserProfile%\.nuget\packages\nuget.commandline\5.4.0\tools\NuGet.exe

IF EXIST "nuget\*" DEL "nuget\*" /Q

dotnet publish DevDB\DevDB.csproj -c Release
"%nuget%" pack DevDB\DevDB.nuspec -OutputDirectory nuget

ENDLOCAL
PAUSE
