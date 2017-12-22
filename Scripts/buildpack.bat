ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
choco install bumpy.portable
msbuild Scripts\updateversionnumber.proj /p:AsmVersion=%VERSION%  
msbuild Scripts\build.proj
nuget pack Scripts\Mapsui.nuspec -Version %VERSION% -outputdirectory Release