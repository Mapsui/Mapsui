# NuGet of Latest Build

On every commit the build server is triggered which publishes a NuGet package of that build. It's version number is the current version of the master branch followed by a dot and the build number. The package is not published to nuget.org but to a AppVeyor feed. You can see the latest packages [here](https://ci.appveyor.com/project/pauldendulk/mapsui/build/artifacts). You need to add this feed to be able to install the package to your project. 

## Add the feed in Visual Studio

Add the feed in tools | options | nuget | package sources 

https://ci.appveyor.com/nuget/mapsui 

![image](https://user-images.githubusercontent.com/963462/159636061-2b66b6f4-1733-45d8-9758-bdaf3a2a716f.png)

## Install the package

```console
PM> nuget install-package mapsui -source https://ci.appveyor.com/nuget/mapsui -pre
```
This installs only the mapsui core package but you could install any other package this way.

## Build your own nuget package
It is also possible to build you own nuget package locally by running ```scripts\buildpack.bat 3.0.0-custom.1``` from the sln folder. You can set the file location as a Package Source in visual studio.
