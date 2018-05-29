# NuGet of Latest Build

On every commit the build server is triggered which publishes a NuGet package of that build. It's version number is the current version of the master branch followed by a dot and the build number. The package is not published to nuget.org but to a AppVeyor feed. You need to add this feed to be able to install the package to your project. There are three ways to add this feed to your environment.

## Package Manager Console

```
nuget install-package mapsui -source https://ci.appveyor.com/nuget/mapsui -pre
```

## Add the feed in Visual Studio

Add the feed in tools | options | nuget | package sources 

https://ci.appveyor.com/nuget/mapsui 

And under 'manage nuget packages' select this as source on the top right.

## Add a .nuget\NuGet.Config xml

In you sln folder add a .nuget folder and in it a NuGet.Config file with content:

```
<?xml version="1.0" encoding="utf-8"?>

<configuration>
  <solution>
    <add key="disableSourceControlIntegration" value="true" />
  </solution>
  <packageSources>
    <add key="AppVeyor" value="https://ci.appveyor.com/nuget/mapsui " />
    <add key="nuget.org" value="https://www.nuget.org/api/v2/" />
  </packageSources>
</configuration>
```
This way you application knows where to find and other checking out your project will get it to build without further configuration. This is the best option.

**Note**: Only the package of the latest build is available. The older packages are removed. So if you have created a project that refers to a package and someone else checks out after there has been an other build, the project will not build because the nuget package is missing. It is easy to update to the latest package, but it is good to be aware of this.

## Build your own nuget package
It is also possible to build you own nuget package locally by running ```scripts\buildpack.bat 1.0.0-beta.1``` from the sln folder. You can set the file location as a Package Source in visual studio.
