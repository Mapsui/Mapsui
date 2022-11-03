ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
dotnet build tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..\bin || exit /B 1
tools\bin\versionupdater -v %VERSION% || exit /B 1

REM create Artifacts if not exists
if not exist "Artifacts" mkdir Artifacts

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui/Mapsui.csproj
.\.nuget\nuget pack .\NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.Rendering.Skia/Mapsui.Rendering.Skia.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Rendering.Skia.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.Tiling/Mapsui.Tiling.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Tiling.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.Nts/Mapsui.Nts.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Nts.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.ArcGIS/Mapsui.ArcGIS.csproj
.\.nuget\nuget pack NuSpec\Mapsui.ArcGIS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.Extensions/Mapsui.Extensions.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Extensions.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Wpf/Mapsui.UI.Wpf.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Wpf.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Android/Mapsui.UI.Android.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Android.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.iOS/Mapsui.UI.iOS.csproj
.\.nuget\nuget pack NuSpec\Mapsui.iOS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Forms/Mapsui.UI.Forms.csproj    
.\.nuget\nuget pack NuSpec\Mapsui.Forms.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Avalonia/Mapsui.UI.Avalonia.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Avalonia.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Eto/Mapsui.UI.Eto.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Eto.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Maui/Mapsui.UI.Maui.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Maui.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

msbuild /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Uno/Mapsui.UI.Uno.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Uno.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet build /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Uno.WinUI/Mapsui.UI.Uno.WinUI.csproj
.\.nuget\nuget pack NuSpec\Mapsui.Uno.WinUI.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

msbuild /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.WinUI/Mapsui.UI.WinUI.csproj
.\.nuget\nuget pack NuSpec\Mapsui.WinUI.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1
