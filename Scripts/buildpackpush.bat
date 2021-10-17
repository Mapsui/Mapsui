SETLOCAL
SET VERSION=%1
CALL Scripts\buildpack %VERSION% || exit /B 1
nuget push .\Artifacts\Mapsui.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.Forms.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.Wpf.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.Avalonia.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.iOS.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.Android.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.Uno.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.Uwp.%VERSION%.nupkg -source nuget.org || exit /B 1
nuget push .\Artifacts\Mapsui.WinUI.%VERSION%.nupkg -source nuget.org || exit /B 1
git commit -m %VERSION% -a || exit /B 1
git tag %VERSION% || exit /B 1
git push origin %VERSION% || exit /B 1
git push || exit /B 1
