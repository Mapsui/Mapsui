choco install docfx
docfx docfx\mapsui\docfx.json
del contributor /s /q 
xcopy docfx\mapsui\_site docs /E /Y
