# Mapsui Contributor Guidelines

## Issue first
Submit an issue before a pull request so we can discuss the possible solutions to the problem.

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

## Keep our direct and indirect dependencies in sync
When we have direct and indirect dependecies on a nuget package those should all refer to the same version. For instance we have a direct dependency on SkiaSharp, but we also use  SvgSkia and RichTextKit and those have a dependency on SkiaSharp too. It would be optimal if all referred to the same version of SkiaSharp. This might not always be possible.

## Extension methods
- Extension methods should always be in a Extensions folder. 
- They should be in a class that has the name '{ClassItExtends}Extensions'. 
- It should be in a namespace that follows the folder name (so not in the namespace of the class it extends).
- Extensions of a collection (IEnumerable, List, Array etc) of a type should also be in the class that extends the individual type.

## Ordering of lon lat
- In our code we prefor a lon, lat order consistent with the x, y order of most cartographic projections.
- Some background: The order of lon and lat always causes a lot of confusion. The official notation is lat, lon, but in map projections the lat corresponds to the y-axis and the lon to the x-axis. This is confusing because in math the ordering is the other way around: x, y. In our code we need to translate the lat/lon to an X/Y coordinate to draw it on the map. In the constructor of such a point the x (lon) will be the first parameter. There is no way that this problem can be fundamentally solved, there will always be some confusion. To mitigate it we choose one way of ordering which is lon, lat (consistent with x, y). 
- Also there are many ways in which we can avoid ordering altogher. For instance if we work with Longitude and Latitude properties. In the case of SphericalMercator.FromLonLat we use lon/lat in the method name to avoid confusion.

## No rendering in the draw/paint loop
Mapsui strives for optiomal performance, so in the rendering loop the objects should be ready to be painted to canvas directly without any need for preparation. This is currently (4.0.0-beta.8) not the case. For instance in the case of tiles they are rendered on the first iteration, after that the cached version is used. This needs to be improved.
### About the terminology
**Rendering**: Create a platform specific resource.
```csharp
SKPath path = ToSKPath(feature, style);
```

**Drawing or Painting**: Draw the platform specific resource to the canvas.
```csharp
canvas.DrawPath(path, paint);
```

## Formatting

We use .editorconfig and we should follow these settings. To apply it in Visual Studio you can select your file and run:: sln explorer menu | Analyze and Code Cleanup | Run Code Cleanup (Profile 1 of 2). You can configure which rules to apply in your profile. For xml (not supported by .editorconfig) two spaces indentation is used.
