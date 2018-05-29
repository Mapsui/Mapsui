choco install docfx -y
choco update docfx -y
docfx docfx\mapsui\docfx.json
del docs /s /q 
xcopy docfx\mapsui\_site docs /E /Y
