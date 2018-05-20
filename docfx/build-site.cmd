choco install docfx
docfx docfx\mapsui\docfx.json
del 
xcopy docfx\mapsui\_site docs /E /Y
git add -A 
git commit -m "Generating site"
git push