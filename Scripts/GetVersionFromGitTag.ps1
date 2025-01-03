# build Samples we# Get the latest Git tag
$gitTag = git describe --tags --abbrev=0

# Save the version to a temporary file
$versionFilePath = "$PSScriptRoot\..\version-from-git-tag.txt"
Set-Content -Path $versionFilePath -Value $gitTag

Write-Host "Version fetched from Git: $gitTag"
