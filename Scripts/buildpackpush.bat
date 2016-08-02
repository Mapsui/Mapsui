@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\..\tools\nuget\nuget.exe

CALL buildpack %VERSION%
%NUGET% push .\..\Release\Mapsui.%VERSION%.nupkg 
git commit -m %VERSION% -a
git tag %VERSION%
git push


