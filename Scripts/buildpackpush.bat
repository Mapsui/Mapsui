SETLOCAL
SET VERSION=%1
CALL Scripts\buildpack %VERSION%
nuget push .\Release\Mapsui.%VERSION%.nupkg -source nuget.org
git commit -m %VERSION% -a
git tag %VERSION%
git push origin %VERSION%
git push
