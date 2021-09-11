ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
msbuild tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..\bin || exit /B 1
tools\bin\versionupdater -v %VERSION% || exit /B 1