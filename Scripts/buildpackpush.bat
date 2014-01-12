@ECHO OFF
SETLOCAL
SET VERSION=%1
SET NUGET=.\..\.nuget\nuget.exe

msbuild /t:BuildRelease .\build.proj /p:AsmVersion=%VERSION%
%NUGET% pack Mapsui.nuspec -Version %VERSION% -outputdirectory .\..\Release
%NUGET% push .\..\Release\Mapsui.%VERSION%.nupkg 


