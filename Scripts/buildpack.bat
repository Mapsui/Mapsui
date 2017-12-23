ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
tools\versionupdater /version:%VERSION%
REM msbuild Scripts\updateversionnumber.proj /p:AsmVersion=%VERSION%  
msbuild Scripts\build.proj
nuget pack NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Release