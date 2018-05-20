choco install docfx
docfx docfx\mapsui\docfx.json
del contributor /s /q 
xcopy docfx\mapsui\_site docs /E /Y
git add -A 
git commit -m "Generating site"
git push