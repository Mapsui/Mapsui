ECHO ON
SETLOCAL
SET VERSION=%1

rmdir obj /s /q
rmdir Release /s /q
choco install bumpy.portable
bumpy write %VERSION%  
REM msbuild Scripts\updateversionnumber.proj /p:AsmVersion=%VERSION%  
REM msbuild Scripts\build.proj
REM nuget pack NuSpec\Mapsui.nuspec -Version %VERSION% -outputdirectory Release