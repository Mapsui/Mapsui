This folder contains shell scripts for building and packaging Mapsui on MacOS.
They should be run from the Mapsui base directory like this:

> Script/Mac/cleanup.sh
(This cleans up all files that are generated during the build.)

> Scripts/Mac/buildpack.sh 3.0.0-alpha.5
(This updates the version to the one specified as argument,
builds everything and creates nuget packages for Mapsui and Mapsui.Forms.)

Not that the nuget packages created in this way will be missing WPF and UWP support.
