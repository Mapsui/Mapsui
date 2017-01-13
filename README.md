[![NuGet Status](http://img.shields.io/nuget/v/Mapsui.svg?style=flat)](https://www.nuget.org/packages/Mapsui/)

## Mapsui (pronounced map-su-wii)

Mapsui is library for apps that need a map

- Is meant for apps
- Designed to be fast and responsive (see [architecture](https://github.com/pauldendulk/Mapsui/wiki/Async-Fetching))
- Cross platform (WPF, UWP, Android, iOS)
- Based on a modified version of SharpMap
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

- Mapsui.UI.{platform} - Platform specific UI. Contains the MapControl
- Mapsui.Rendering.Skia [1]
- Mapsui [1]
- Mapsui.Geometries [1]
- On Windows dekstop Mapsui.Rendering.Xaml is added.
- A number of nuget dependencies

[1] A PCL with Profile111 targets: .Net Framework 4.5, ASP.NET Core 5.0, Windows 8, Windows Phone 8.1, Xamarin.Android, Xamarin.iOS, Xamarin.iOS (Classic)

## Wiki
Please take a look at the [wiki](https://github.com/pauldendulk/Mapsui/wiki). Please let us know what information you are missing. If you have a question please submit an [issue](https://github.com/pauldendulk/Mapsui/issues) or a question on stackoverflow with the 'mapsui' tag (I will get a notification).

## License 

LGPL
