# Mapsui Contributor Guidelines

Note: Below are some contributor guidelines. Items were added when we felt it was useful. It is not intended to be some complete list.

## If you want to contribute you need to sign the Contributor License Agreement (CLA)

[![CLA assistant](https://cla-assistant.io/readme/badge/Mapsui/Mapsui)](https://cla-assistant.io/Mapsui/Mapsui)

## Issues first

Submit an issue before a pull request so we can discuss the possible solutions to the problem.

## Create small PRs that are about one topic

To be able to review a PR it helps if it is a small change which covers only one topic. For the reviewer it is important to understand the purpose. Take the reviewer along in your own thinking steps. There was a problem, you considered solutions, and there was a reason why you came to this solution. The diff shows what has changed, so it is not important to explain this (but it is useful to summarize it). What should be clear from the PR description, commits and code comments is the 'why'.

### For bigger changes work with PR dependencies

If you create a PR and want to make additional changes based on the earlier changes you should create a new PR that depends and the previous PR. Mention in the description that it depends on the other PR. Once the previous PR was merged you can update the next one with `git pull origin main` on the commend line. If you do not really need PR dependencies (because the changes are not really related) then it is preferred to create independent PRs.

## Formatting

We use [.editorconfig](https://editorconfig.org) in our repository and the code should conform with this. 

In Visual Studio you can check for complicance with the context menu in the Solution Explorer and select `Analyze and Code Cleanup` and then `Run Code Analysis`. The results will show up in the `Error List` with `Build and IntelliSense` selected.  

Alternatively you can use the [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) command.

## All checks should be green all the time

At all times:

- All projects should compile
- The unit tests should succeed
- All samples should run properly

## Keep the core code simple and move potential problematic code up to the surface projects
Code depends on other code. In a hierarchy it is better to have high quality code which is reliable and preditable in the core. You don't want potential problems to be propagated from the core to all other code because then everything becomes problematic. Potential problems are:

- IDisposable objects. You don't want callers to have to deal with that. You don't want other code to have to decide about disposing or not.
- Code that is async/await. Synchronous code is simpler. Calls are async/await because of long lasting calls which is often because of calls to external services. Those depend on network which could be slow or missing and the size of the data which could be bigger than expected.
- Code that could throw an exception, so the caller has to catch it, or perhaps it should not need to catch it (it is better if you do not have to decide).
- Fields that are nullable. Check for null early on the surface before passing it along to the core.

A practical example in our own code is the dependency of: DataSource <- Fetcher <- Layer <- Map <- MapControl. The DataSource is disposable and because of this all other classes become disposable. We want to improve this by moving the disposable parts to a centralized data fetcher.

## Prefer pure functions and immutable data

This is related to the paragraph above. In general it is considered to be a good thing to use pure functions, however, you may need to reorganize things at a more global scale to make this possible.

## Extension methods

- Extension methods should always be in an 'Extensions' folder. 
- They should be in a class that has the name '{ClassItExtends}Extensions'. 
- It should be in a namespace that follows the folder name (so not in the namespace of the class it extends).
- Extensions of a collection (IEnumerable, List, Array etc) of a type should also be in the class that extends the individual type.
- If an interface is extended, the `I` should not be part of the class name. So a class with extensions of ILayer should be called LayerExtensions.

## Direct and indirect nuget dependencies should point to the same version

For instance, we have a direct dependency on SkiaSharp, but we also use Svg.Skia which has a dependency on SkiaSharp too. We want both of those to point to the same version. If not, linking or runtime errors could occur. In some cases this might not be possible, for instance, when we have to downgrade because of a bug in some package.

## PR titles should be written as release notes

Github can generate release notes from PR titles, so the PR titles should be written as release notes entries. Let's do it like this:

- Use imperative mood. See something about this in [this post](https://www.freecodecamp.org/news/how-to-write-better-git-commit-messages/) about commit messages (which should also use imperative mood). Most of the time the title will begin with a verb, like Fix, Update or Add.
- Don't use the number of the issue it is referencing in the title but in the branch name (in the format suggested by github when you click the 'create a branch' link next to an issue). It will automatically be associated with the issue.
- The title should be self explanatory and the interpretation should not depend on the content of the issue it is referring to.
- The title should be succinct. It can not always be a full description. The users can read the rest in the PR itself. There is a link to the PR next to the entry.

## Ordering of lon lat

- In our code we prefer a lon, lat order consistent with the x, y order of most cartographic projections.
- Some background: The order of lon and lat always causes a lot of confusion. The official notation is lat, lon, but in map projections the lat corresponds to the y-axis and the lon to the x-axis. This is confusing because in math the ordering is the other way around: x, y. In our code we need to translate the lat/lon to an X/Y coordinate to draw it on the map. In the constructor of such a point the x (lon) will be the first parameter. There is no way that this problem can be fundamentally solved, there will always be some confusion. To mitigate it we choose one way of ordering which is lon, lat (consistent with x, y). 
- Also there are many ways in which we can avoid ordering altogether. For instance if we work with Longitude and Latitude properties. In the case of SphericalMercator.FromLonLat we use lon/lat in the method name to avoid confusion.

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

## Keep the renderer behind and interface

As of v4 Mapsui has only one renderer, SkiaSharp. Although we have only one renderer and have currently no plans to add others we will keep it behind an interface. There are costs to keeping the abstraction but we keep it because a change could happen again, even though it does not seem likely now. In the past we had to switch renderers many times, a list:

- System.Drawing
- System.Drawing for PocketPC
- Silverlight XAML
- WPF XAML
- UWP XAML (could later be merged with WPF XAML)
- iOS native rendering
- Android native rendering (this is actually internally using skia)
- OpenTK (this was not mature enough at that point)
- SkiaSharp

## Mapsui should not be limited to a single coordinate system

Mapsui's Map can be in any coordinate system. If you do not specify a coordinate system in the Map and Layers it assumes they are in the same coordinate system (whatever they are). In this case it only transforms these unspecified 'world-coordinates' to 'screen-coordinates' and nothing more. It is also possible to setup a coordinate transformation system using Map.CRS, DataSource.CRS and Map.Transformation. See [projections](projections.md).

## Full implementation of the feature matrix

These are some of the feature dimensions:

- Renderers: WPF and Skia
- Geometries: Point, LineString, Polygon etc.
- Operations on Geometries: Distance, Contains.
- Coordinate projection support
- Style: fill color, line color, line cap, symbol opacity, symbol scale 

If we choose to support a feature each 'cell' of the multi dimensional matrix should be supported. No surprises for the user. At the moment (v4.1.0) there are holes in the matrix on some point (like differences between the various platforms). 

## Put effort in keeping things simple

Growing complexity is one of the biggest problems in software development. To keep this project maintainable we should put effort in keeping the complexity low. Complexity can be caused by clueless spaghetti code but also by [astronaut architectures](https://www.joelonsoftware.com/2008/05/01/architecture-astronauts-take-over/). Keeping things simple is [not easy](https://www.infoq.com/presentations/Simple-Made-Easy) but hard work. It involves thinking up several solutions to your problem weighing the pros and cons and moving it around and upside down to look for even better (simpler) solutions. 

## Continuous Refactoring

Mapsui contains some older code. Don't despair. We continuously improve or replace older code. It is a gradual process. We do it step by step. Although the steps are small we have managed to make major changes in the past: from WinForms to WPF, from GDI+ to SL rendering, from .NET Framework to PCL, from PCL to .NET Standard, from WPF rendering to SkiaSharp, from Mapsui geometries to NTS. Taking such steps results in breaking changes. We are aware of this and clearly communicate it with the user. We use [semver](http://semver.org) so breaking changes go in to major version upgrades.
