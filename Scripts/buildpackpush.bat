@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\..\.nuget\nuget.exe

buildpack %VERSION%
%NUGET% push .\..\Release\Mapsui.%VERSION%.nupkg 


