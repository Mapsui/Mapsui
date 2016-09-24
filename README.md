[![NuGet Status](http://img.shields.io/nuget/v/Mapsui.svg?style=flat)](https://www.nuget.org/packages/Mapsui/)

## Mapsui (pronounced map-su-wii) 

Mapsui is a C# library for mapping applications.

- Intended to develop apps
- Designed to be fast and responsive (see [architecture](https://github.com/pauldendulk/Mapsui/wiki/Async-Fetching))
- All data fetching is on a background thread (disk, web, or database)
- Code is written to be used cross platform (using PCL or code sharing)
- Based on a modified version of SharpMap. 
- Uses BruTile to access tile services

### Get it from NuGet 
`
PM> Install-Package Mapsui
`

https://www.nuget.org/packages/Mapsui

## Platforms Supported

There are four platforms supported:
- Windows Desktop - WPF on .NET 4.5
- Windows Store - Profile32 PCLs (Windows 8.1 and Windows Phone 8.1)
- Mapsui Android - for API Level 15 (v4.0.3 - Ice Cream Sandwich)
- Mapsui iOS

If you install the NuGet package into your app three assemblies are added:

- Mapsui - The core project, is a PCL with Profile111 which targets (.Net Framework 4.5, ASP.NET Core 5.0, Windows 8, Windows Phone 8.1, Xamarin.Android, Xamarin.iOS, Xamarin.iOS (Classic))
- Mapsui.Rendering - A platform specific renderer
- Mapsui.UI - Which contains the MapControl for that platform

## Warnings

- There is limited documentation.
- Breaking changes will be introduced frequently. We change the API whenever we feel this is an improvement.
- We adopt new technologies relatively fast, and dropping support for older frameworks.
- Only use this project if you are willing to dig into the code.
- Although there is a general plan of where to go with this library there are not enough resources to go towards that goal in a systematic way. New functionality is driven by what is needed in our projects.

## License 

LGPL
