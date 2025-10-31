# Install .NET 6 SDK using winget (uncomment if needed)
# Currently mkdocs needs to be installed manually. Perhaps it is better to install it in this script.

Write-Output "Create general documentation with mkdocs in 'docs/general/_site'"
mkdocs build -f docs/general/mkdocs.yml

Write-Output "Installing DocFX"
dotnet tool update -g docfx --version 2.78

Write-Output "Create api documentation with docfx in 'docs/api/_site'"
docfx docs/api/docfx.json

Write-Output "Deleting existing 'website'' folder and contents"
Remove-Item -Path "website" -Recurse -Force -ErrorAction SilentlyContinue

Write-Output "Creating a new 'website' and folders"
New-Item -ItemType Directory -Path "website"
New-Item -ItemType Directory -Path "website/v5"
New-Item -ItemType Directory -Path "website/v5/api"

Write-Output "Copying CNAME file to 'website'. This is necessary for the mapsui.com domain"
Copy-Item -Path "docs/CNAME" -Destination "website" -Force

Write-Output "Copying general '_site' to 'website'"
Copy-Item -Path "docs/general/_site/*" -Destination "website/v5" -Recurse -Force

Write-Output "Copying api '_site' to 'website'"
Copy-Item -Path "docs/api/_site/*" -Destination "website/v5/api" -Recurse -Force

Write-Output "Copying main ''docs/index.html' to 'website/index.html'"
Copy-Item -Path "docs/index.html" -Destination "website/index.html" -Recurse -Force

