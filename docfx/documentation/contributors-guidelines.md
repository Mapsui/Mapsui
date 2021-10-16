# Mapsui Contributor Guidelines

## Issue first
Submit an issue before a pull request co we can discuss the possible solutions to the problem.

## Sign the Contributor License Agreement (CLA)
To contribute you need to sign our CLA 

[![CLA assistant](https://cla-assistant.io/readme/badge/Mapsui/Mapsui)](https://cla-assistant.io/Mapsui/Mapsui)

## Complexity
Complexity is the biggest problem in software development. The primary effort should be to keep the complexity low. Complexity can be caused by clueless spaghetti code but also by [astronaut architectures](https://www.joelonsoftware.com/2008/05/01/architecture-astronauts-take-over/). Keeping things simple is [not easy](https://www.infoq.com/presentations/Simple-Made-Easy) but hard work. It involves thinking up several solutions to your problem weighing the pros and cons and moving it around and upside down to look for even better (simpler) solutions. 

## Continuous Refactoring
Mapsui has some older code in it. Don't despair. We continuously improve or replace older code. It is a gradual process. We do it step by step. We have made major changes in the past; From WinForms to WPF, From GDI+ to SL rendering. From .NET Framework to PCL. From PCL to .NET Standard. From WPF rendering to SkiaSharp. Add support for Xamarin.Forms. Future changes will include moving to NTS geometries, improving the Layers list. Taking these steps will cause breaking changes. We are aware of this and clearly communicate it with the user. We use [semver](http://semver.org) so breaking changes go in to major version upgrades.

## All checks should be green all the time
At all times:
- All projects should compile
- The unit tests should succeed
- All samples should run properly

## Use ReSharper
Mapsui uses the resharper team settings by committing the DotSettings to git, so that all developers can use the same settings. We should have zero warnings. Suggestions should be treated as actual suggestions, use them only when you think it improves the code.

## Keep dependencies in the csproj and nuspec in sync
If we upgrade a nuget package in the solution we should also upgrade the lower bound of the nuget package in the nuspec because this is what nuget installs. We want the user to get the same version as the one that we are working and testing with.

example csproj
```xml
<PackageReference Include="SkiaSharp" Version="2.80.2" />
```
exmple nuspec
```
<dependency id="SkiaSharp" version="[2.80.2,3.0.0)"/>
```

## Keep our direct and indirect dependencies in sync
When we have direct and indirect dependecies on a nuget package those should all refer to the same version. For instance we have a direct dependency on SkiaSharp, but we also use  SvgSkia and RichTextKit and those have a dependency on SkiaSharp too. It would be optimal if all referred to the same version of SkiaSharp. This might not always be possible.

## Extension methods
- Extension methods should always be in a Extensions folder. 
- They should be in a class that has the name '{ClassItExtents}Extensions'. 
- It should be in a namespace that follows the folder name (so not in the namespace of the class it extents).
