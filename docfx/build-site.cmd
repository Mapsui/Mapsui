choco install docfx -y
choco update docfx -y
docfx docfx\docfx.json
del docs /s /q 
xcopy docfx\_site docs /E /Y
