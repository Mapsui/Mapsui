[![NuGet Status](http://img.shields.io/nuget/v/Mapsui.svg?style=flat)](https://www.nuget.org/packages/Mapsui/)

## Mapsui (pronounced map-su-wii)

Mapsui is library for apps that need a map

- Intended to develop apps
- Designed to be fast and responsive (see [architecture](https://github.com/pauldendulk/Mapsui/wiki/Async-Fetching))
- All data fetching is on a background thread (disk, web, or database)
- Code is written to be used cross platform (using PCL or code sharing)
- Based on a modified version of SharpMap. 
- Uses BruTile to access tile services

## Getting Started

Look [here](https://github.com/pauldendulk/Mapsui/wiki/Getting-Started-with-Mapsui)

## Get it from NuGet 
```
PM> Install-Package Mapsui
```

https://www.nuget.org/packages/Mapsui

## Platforms Supported

There are four platforms supported:
- **WPF** - Windows Desktop on .NET 4.5.2
- **UWP** - Windows Store on Windows 10 build 10586
- **Android** - Xamarin.Android on API Level 19 (v4.4 - Kit Kat)
- **iOS** - Xamarin.iOS

## Components

If you install the NuGet package into your app these assemblies are added:

- Mapsui.UI - Platorm specific UI. Contains the MapControl
- Mapsui.Rendering - A platform specific renderer
- Mapsui - A PCL with Profile111 
- Mapsui.Geometries - A PCL with Profile111

Profile111 targets: .Net Framework 4.5, ASP.NET Core 5.0, Windows 8, Windows Phone 8.1, Xamarin.Android, Xamarin.iOS, Xamarin.iOS (Classic)

## Wiki
Take a look at the [wiki](https://github.com/pauldendulk/Mapsui/wiki). We are starting to add some information there. If you have a question please submit an [issue](https://github.com/pauldendulk/Mapsui/issues) or a question on stackoverflow the the 'mapsui' tag (I will get a notification).

## License 

LGPL
