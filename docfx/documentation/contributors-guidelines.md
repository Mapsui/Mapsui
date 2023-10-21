# Mapsui Contributor Guidelines

Note: Below are some contributor guidelines. Items were added when we felt it was useful. It is not intended to be some complete list.

## If you want to contribute you need to sign the Contributor License Agreement (CLA)

[![CLA assistant](https://cla-assistant.io/readme/badge/Mapsui/Mapsui)](https://cla-assistant.io/Mapsui/Mapsui)

## Issues first
Submit an issue before a pull request so we can discuss the possible solutions to the problem.

## All checks should be green all the time
At all times:
- All projects should compile
- The unit tests should succeed
- All samples should run properly

## Our direct and indirect nuget dependencies should point to the same version
For instance, we have a direct dependency on SkiaSharp, but we also use Svg.Skia which has a dependency on SkiaSharp too. We want both of those to point to the same version. If not, linking or runtime errors could occur. In some cases this might not be possible, for instance, when we have to downgrade because of a bug in some package.

## Extension methods
- Extension methods should always be in an 'Extensions' folder. 
- They should be in a class that has the name '{ClassItExtends}Extensions'. 
- It should be in a namespace that follows the folder name (so not in the namespace of the class it extends).
- Extensions of a collection (IEnumerable, List, Array etc) of a type should also be in the class that extends the individual type.
- If an interface is extended, the I should not be in the class name. So and ILayer extension should be LayerExtensions.

## Ordering of lon lat
- In our code we prefor a lon, lat order consistent with the x, y order of most cartographic projections.
- Some background: The order of lon and lat always causes a lot of confusion. The official notation is lat, lon, but in map projections the lat corresponds to the y-axis and the lon to the x-axis. This is confusing because in math the ordering is the other way around: x, y. In our code we need to translate the lat/lon to an X/Y coordinate to draw it on the map. In the constructor of such a point the x (lon) will be the first parameter. There is no way that this problem can be fundamentally solved, there will always be some confusion. To mitigate it we choose one way of ordering which is lon, lat (consistent with x, y). 
- Also there are many ways in which we can avoid ordering altogher. For instance if we work with Longitude and Latitude properties. In the case of SphericalMercator.FromLonLat we use lon/lat in the method name to avoid confusion.

## No rendering in the draw/paint loop
Mapsui strives for optiomal performance, so in the rendering loop the objects should be ready to be painted to canvas directly without any need for preparation. This is currently (4.1.0) not the case. For instance in the case of tiles they are rendered on the first iteration, after that the cached version is used. This needs to be improved.
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
We use [.editorconfig](https://editorconfig.org) in our repository and the code should comply to this. To apply specifc rules in Visual Studio you can select a project | context menu | Analyze and Code Cleanup | Run Code Cleanup (Profile 1 of 2). You can configure which rules to apply in these profiles. Alternatively you can use the [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) command.

## PR titles should be written as release notes
Github can generate release notes from PR titles, so the PR titles should be written as release notes entries. Let's do it like this:
- Use imperative mood. See something about this in [this post](https://www.freecodecamp.org/news/how-to-write-better-git-commit-messages/) about commit messages (which should also use imperative mood). Most of the time the title will begin with a verb, like Fix, Update or Add.
- Don't use the number of the issue it is referencing in the title but in the branch name (in the format suggested by github when you click the 'create a branch' link next to an issue). It will automatically be associated with the issue.
- The title should be self explanatory and the interpretation should not depend on the content of the issue it is referring to.
- The title should be succinct. It can not always be a full description. The users can read the rest in the PR itself. There is a link to the PR next to the entry.

## Put effort in keeping things simple
Growing complexity is one of the biggest problem in software development. To keep this project maintainable we should put effort in keeping the complixity low. Complexity can be caused by clueless spaghetti code but also by [astronaut architectures](https://www.joelonsoftware.com/2008/05/01/architecture-astronauts-take-over/). Keeping things simple is [not easy](https://www.infoq.com/presentations/Simple-Made-Easy) but hard work. It involves thinking up several solutions to your problem weighing the pros and cons and moving it around and upside down to look for even better (simpler) solutions. 

## Continuous Refactoring
Mapsui contains some older code. Don't despair. We continuously improve or replace older code. It is a gradual process. We do it step by step. Although the steps are small we have managed to make major changes in the past: from WinForms to WPF, from GDI+ to SL rendering, from .NET Framework to PCL, from PCL to .NET Standard, from WPF rendering to SkiaSharp, from Mapsui geometries to NTS. Taking such steps results in breaking changes. We are aware of this and clearly communicate it with the user. We use [semver](http://semver.org) so breaking changes go in to major version upgrades.

