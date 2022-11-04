ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
dotnet build tools\versionupdater\versionupdater.csproj /p:Configuration=Release /p:OutputPath=..\bin || exit /B 1
tools\bin\versionupdater -v %VERSION% || exit /B 1

REM create Artifacts if not exists
if not exist "Artifacts" mkdir Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui/Mapsui.csproj --output Artifacts
REM .\.nuget\nuget pack .\NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Rendering.Skia/Mapsui.Rendering.Skia.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Rendering.Skia.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Tiling/Mapsui.Tiling.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Tiling.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Nts/Mapsui.Nts.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Nts.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.ArcGIS/Mapsui.ArcGIS.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.ArcGIS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Extensions/Mapsui.Extensions.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Extensions.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Wpf/Mapsui.UI.Wpf.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Wpf.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Android/Mapsui.UI.Android.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Android.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.iOS/Mapsui.UI.iOS.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.iOS.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Forms/Mapsui.UI.Forms.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Forms.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Avalonia/Mapsui.UI.Avalonia.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Avalonia.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Eto/Mapsui.UI.Eto.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Eto.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Maui/Mapsui.UI.Maui.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Maui.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

msbuild /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Uno/Mapsui.UI.Uno.csproj /t:Pack 
xcopy Mapsui.UI.Uno\bin\Release\*.nupkg Artifacts /Y
REM .\.nuget\nuget pack NuSpec\Mapsui.Uno.nuspec -Prop Configuration=Release -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Uno.WinUI/Mapsui.UI.Uno.WinUI.csproj --output Artifacts
REM .\.nuget\nuget pack NuSpec\Mapsui.Uno.WinUI.nuspec -Version %VERSION% -outputdirectory Artifacts  || exit /B 1

msbuild /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.WinUI/Mapsui.UI.WinUI.csproj /t:Pack
xcopy Mapsui.UI.WinUI\bin\Release\*.nupkg Artifacts /Y
REM .\.nuget\nuget pack Mapsui.UI.WinUI/Mapsui.UI.WinUI.csproj -Version %VERSION% -outputdirectory Artifacts || exit /B 1
