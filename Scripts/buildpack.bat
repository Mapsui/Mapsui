@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\..\.nuget\nuget.exe

rmdir .\..\obj /s /q
rmdir .\..\release /s /q
msbuild updateversionnumber.proj /p:AsmVersion=%VERSION%
msbuild build.proj 
%NUGET% pack Mapsui.nuspec -Version %VERSION% -outputdirectory .\..\Release


