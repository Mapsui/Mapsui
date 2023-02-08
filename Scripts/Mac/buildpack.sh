# REM We have a new way of releasing therefor I remove the nuspec and VersionUpdater references. 
# REM Not sure if there is a need for this script in the current situation. Mac developers should tell me (pauldendulk).

#!/bin/zsh
VERSION=$1

# clean up
find . -type d \( -name 'bin' -o -name 'obj' \) -prune -exec rm -rf {} \;
nuget restore Mapsui.Mac.sln

# build
msbuild Mapsui.Mac.sln /p:Configuration=Release
