ECHO CHOCO INSTALL
choco install docfx --version 2.48.0 --allow-downgrade  -y || exit /b 
ECHO NUGET RESTORE
nuget restore mapsui.sln 
ECHO DOCFX
docfx docfx\docfx.json || exit /b 
ECHO DELETE docs 
del docs /s /q || exit /b 
REM the CNAME file is generated in the doc folder when added through the github 
REM settings page. Apparently github uses this file to determine the custom 
REM domain. The line above deletes the whole docs folder so we need to copy it 
REM from the docfx folder. If we ever want to change the custom domain settings
REM we need to alter the docfx\CNAME file and not use the github settings page.
ECHO COPY CNAME
copy docfx\CNAME docs /Y || exit /b 
ECHO COPY _site TO docs
xcopy docfx\_site docs /E /Y || exit /b 
