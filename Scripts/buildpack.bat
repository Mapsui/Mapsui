ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
msbuild tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..\bin || exit /B 1
tools\bin\versionupdater -v %VERSION% || exit /B 1
msbuild.exe mapsui.sln /p:Configuration=Release /t:restore  || exit /B 1
msbuild.exe mapsui.sln /p:Configuration=Release  || exit /B 1
nuget pack NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Forms.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Wpf.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Avalonia.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.iOS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Android.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Uno.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Uwp.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.WinUI.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
