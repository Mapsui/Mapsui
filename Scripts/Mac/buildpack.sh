#!/bin/zsh
VERSION=$1

# clean up
find . -type d \( -name 'bin' -o -name 'obj' \) -prune -exec rm -rf {} \;
nuget restore Mapsui.Mac.sln
# update version
msbuild Tools/VersionUpdater/VersionUpdater.csproj /p:Configuration=Release
dotnet Tools/VersionUpdater/bin/Release/netcoreapp3.1/VersionUpdater.dll -v $VERSION
# build
msbuild Mapsui.Mac.sln /p:Configuration=Release
# package
nuget pack NuSpec/Mapsui.nuspec -Version $VERSION -outputdirectory Artifacts
nuget pack NuSpec/Mapsui.Forms.nuspec -Version $VERSION -outputdirectory Artifacts
nuget pack NuSpec/Mapsui.Android.nuspec -Version $VERSION -outputdirectory Artifacts
nuget pack NuSpec/Mapsui.iOS.nuspec -Version $VERSION -outputdirectory Artifacts
