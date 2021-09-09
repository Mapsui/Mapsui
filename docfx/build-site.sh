#!/bin/zsh
nuget restore Mapsui.Mac.sln
docfx docfx/docfx.json
rm -rf docs/*
# the CNAME file is generated in the doc folder when added through the github
# settings page. Apparently github uses this file to determine the custom
# domain. The line above deletes the whole docs folder so we need to copy it
# from the docfx folder. If we ever want to change the custom domain settings
# we need to alter the docfx\CNAME file and not use the github settings page.
cp docfx/CNAME docs
cp -r docfx/_site/* docs
