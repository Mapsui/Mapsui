@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\..\tools\nuget\nuget.exe

CALL buildpack %VERSION%
ECHO buildpack done
%NUGET% push .\..\Release\Mapsui.%VERSION%.nupkg -source nuget.org
ECHO nuget push done
git commit -m %VERSION% -a
ECHO git commit done
git tag %VERSION%
git push origin %VERSION=%1
ECHO git tag done
git push
ECHO git push done