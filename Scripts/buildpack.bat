ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
dotnet build tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..\bin || exit /B 1
tools\bin\versionupdater -v %VERSION% || exit /B 1

REM Replacing the sln build with csproj build because not all projects build atm.
REM dotnet build mapsui.sln /p:Configuration=Release /t:restore  || exit /B 1
REM dotnet build mapsui.sln /p:Configuration=Release  || exit /B 1

dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui/Mapsui.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.Rendering.Skia/Mapsui.Rendering.Skia.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.Tiling/Mapsui.Tiling.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.Nts/Mapsui.Nts.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.ArcGIS/Mapsui.ArcGIS.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.Extensions/Mapsui.Extensions.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.Wpf/Mapsui.UI.Wpf.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.Android/Mapsui.UI.Android.csproj
REM dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.iOS/Mapsui.UI.iOS.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.Forms/Mapsui.UI.Forms.csproj    
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.Avalonia/Mapsui.UI.Avalonia.csproj
REM dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.MAUI/Mapsui.UI.MAUI.csproj
REM dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.Uno/Mapsui.UI.Uno.csproj
REM dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.WinUI/Mapsui.UI.WinUI.csproj
dotnet build /p:RestorePackages=false /p:Configuration=Release Mapsui.UI.Eto/Mapsui.UI.Eto.csproj


nuget pack NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.ArcGIS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Extensions.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Wpf.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Android.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
REM nuget pack NuSpec\Mapsui.iOS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Forms.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Avalonia.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
REM nuget pack NuSpec\Mapsui.MAUI.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
REM nuget pack NuSpec\Mapsui.Uno.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
REM nuget pack NuSpec\Mapsui.WinUI.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
nuget pack NuSpec\Mapsui.Eto.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1