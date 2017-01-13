@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=tools\nuget\nuget.exe

rmdir obj /s /q
rmdir release /s /q
msbuild Scripts\updateversionnumber.proj /p:AsmVersion=%VERSION%
msbuild Scripts\build_with_ios.proj 
%NUGET% pack Scripts\Mapsui.nuspec -Version %VERSION% -outputdirectory Release