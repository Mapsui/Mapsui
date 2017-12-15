@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\tools\nuget\nuget.exe

CALL Scripts\buildpack %VERSION%-beta
ECHO buildpack done
%NUGET% push .\Release\Mapsui.%VERSION%-beta.nupkg -source nuget.org
ECHO nuget push done
git commit -m %VERSION%-beta -a
ECHO git commit done
git tag %VERSION%-beta
git push origin %VERSION%-beta
ECHO git tag done
git push
ECHO git push done