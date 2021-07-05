#!/bin/zsh
VERSION=$1

# clean up
find . -type d \( -name 'bin' -o -name 'obj' \) -prune -exec rm -rf {} \;
nuget restore Mapsui.Mac.sln
# update version
msbuild Tools/VersionUpdater/VersionUpdater.csproj /p:Configuration=Release
mono Tools/VersionUpdater/bin/Release/net48/win-x64/VersionUpdater.exe /version:$VERSION
# build
msbuild Mapsui.Mac.sln /p:Configuration=Release
# package
nuget pack NuSpec/Mapsui.Mac.nuspec -Version $VERSION -outputdirectory Artifacts
nuget pack NuSpec/Mapsui.Forms.nuspec -Version $VERSION -outputdirectory Artifacts
