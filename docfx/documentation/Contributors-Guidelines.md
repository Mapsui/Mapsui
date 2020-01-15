# Mapsui Contributor Guidelines

### CLA
To contribute you need to sign our CLA 

[![CLA assistant](https://cla-assistant.io/readme/badge/Mapsui/Mapsui)](https://cla-assistant.io/Mapsui/Mapsui)
## Complexity
Complexity is the biggest problem in software development. The primary effort should be to keep the complexity low. Complexity can be caused by clueless spaghetti code but also by [astronaut architectures](https://www.joelonsoftware.com/2008/05/01/architecture-astronauts-take-over/). Keeping things simple is [not easy](https://www.infoq.com/presentations/Simple-Made-Easy) but hard work. It involves thinking up several solutions to your problem weighing the pros and cons and moving it around and upside down to look for even better (simpler) solutions. 

## Continuous Refactoring
Mapsui has some older code in it. Don't despair. We continuously improve or replace older code. It is a gradual process. We do it step by step. We have made major changes in the past; From WinForms to WPF, From GDI+ to SL rendering. From .NET Framework to PCL. From WPF rendering to SkiaSharp. We are working toward the next steps: From PCL to .NET Standard, support for full Xamarin.Forms. Taking these steps will cause breaking changes. We should be aware of this and clearly communicate it with the user. We should use [semver](http://semver.org) (although we still violate it now and then).

## Issue first
Submit an issue before a pull request co we can discuss the possible solutions to the problem.

## All checks should be green all the time
At all times:
- All projects should compile
- The unit tests should succeed
- All samples should run properly

## Use ReSharper
Mapsui uses the resharper team settings by committing the DotSettings to git, so that all developers can use the same settings. We should have zero warnings. Suggestions should be treated as actual suggestions, use them only when you think it improves the code.
