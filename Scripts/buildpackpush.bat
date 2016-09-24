@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\..\tools\nuget\nuget.exe

CALL buildpack %VERSION%
ECHO buildpack done
%NUGET% push .\..\Release\Mapsui.%VERSION%.nupkg 
ECHO nuget push done
git commit -m %VERSION% -a
ECHO git commit done
git tag %VERSION%
ECHO git tag done
git push
ECHO git push done


