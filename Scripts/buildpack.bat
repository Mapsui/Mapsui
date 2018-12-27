ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
msbuild tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..
tools\versionupdater /version:%VERSION%
msbuild.exe mapsui.sln /p:Configuration=Release /t:restore
msbuild.exe mapsui.sln /p:Configuration=Release
nuget pack NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Artifacts
nuget pack NuSpec\Mapsui.Forms.nuspec -Version %VERSION% -outputdirectory Artifacts