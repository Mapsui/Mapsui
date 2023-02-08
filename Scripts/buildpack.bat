ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q

REM create Artifacts if not exists
if not exist "Artifacts" mkdir Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui/Mapsui.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Rendering.Skia/Mapsui.Rendering.Skia.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Tiling/Mapsui.Tiling.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Nts/Mapsui.Nts.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.ArcGIS/Mapsui.ArcGIS.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.Extensions/Mapsui.Extensions.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Wpf/Mapsui.UI.Wpf.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Android/Mapsui.UI.Android.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.iOS/Mapsui.UI.iOS.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Blazor/Mapsui.UI.Blazor.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Forms/Mapsui.UI.Forms.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Avalonia/Mapsui.UI.Avalonia.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Eto/Mapsui.UI.Eto.csproj --output Artifacts

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Maui/Mapsui.UI.Maui.csproj --output Artifacts

msbuild /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Uno/Mapsui.UI.Uno.csproj /t:Pack 
xcopy Mapsui.UI.Uno\bin\Release\*.nupkg Artifacts /Y

dotnet pack /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.Uno.WinUI/Mapsui.UI.Uno.WinUI.csproj --output Artifacts

msbuild /p:RestorePackages=true /p:Configuration=Release Mapsui.UI.WinUI/Mapsui.UI.WinUI.csproj /t:Pack
xcopy Mapsui.UI.WinUI\bin\Release\*.nupkg Artifacts /Y