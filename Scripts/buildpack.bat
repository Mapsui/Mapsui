ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
msbuild tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..
tools\versionupdater /version:%VERSION%
msbuild Scripts\build.proj
nuget pack NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Release