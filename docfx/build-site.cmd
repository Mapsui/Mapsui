choco install docfx -y
choco update docfx -y
docfx docfx\docfx.json
del docs /s /q 
REM the CNAME file is generated in the doc folder when added through the github 
REM settings page. Apparently github uses this file to determine the custom 
REM domain. The line above deletes the whole docs folder so we now we copy it 
REM from the docfx folder. If we ever want to change the custom domain settings
REM we need to alter the docfx\CNAME file and not use the github settings page.
copy docfx\CNAME docs /Y
xcopy docfx\_site docs /E /Y
