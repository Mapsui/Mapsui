
REM ECHO INSTALL .NET 6 SDK (was necessary on my machine where I develop all kinds of .NET apps)
REM Perhaps we need: winget install Microsoft.DotNet.SDK.6 --silent
ECHO INSTALL docfx
dotnet tool update -g docfx --version 2.60.2  || exit /b 
ECHO Generate website in docfx\_site folder
docfx docfx\docfx.json || exit /b 
del website /s /q
mkdir website
ECHO COPY CNAME file which is used by github to determine the domain name.
copy docfx\CNAME website /Y || exit /b 
ECHO COPY _site TO website
xcopy docfx\_site website /E /Y || exit /b 
