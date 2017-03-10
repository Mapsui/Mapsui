[![NuGet Status](http://img.shields.io/nuget/v/Mapsui.svg?style=flat)](https://www.nuget.org/packages/Mapsui/)
[![Build status](https://ci.appveyor.com/api/projects/status/p20w43qv4ixkkftp?svg=true)](https://ci.appveyor.com/project/pauldendulk/mapsui)
[![Build Status](https://www.bitrise.io/app/f6f2ae30c3eb921b.svg?token=HqPHuFR_4KakFkxNuh4D-g&branch=master)](https://www.bitrise.io/app/f6f2ae30c3eb921b)

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

## Platforms Supported

- **WPF** - Windows Desktop on .NET 4.5.2
- **UWP** - Windows Store on Windows 10 build 10586
- **Android** - Xamarin.Android on API Level 19 (v4.4 - Kit Kat)
- **iOS** - Xamarin.iOS

## Components

If you install the NuGet package into your app these assemblies are added:

- Mapsui.UI.{platform} - Platform specific UI. Contains the MapControl
- Mapsui.Rendering.Skia \*
- Mapsui \*
- Mapsui.Geometries \*
- On Windows dekstop Mapsui.Rendering.Xaml is added.
- A number of nuget dependencies

\* A PCL with Profile111. Targets: .Net Framework 4.5, ASP.NET Core 5.0, Windows 8, Windows Phone 8.1, Xamarin.Android, Xamarin.iOS, Xamarin.iOS (Classic)

## Wiki
Please take a look at the [wiki](https://github.com/pauldendulk/Mapsui/wiki). Let us know what information you are missing for your projects. 

## Questions
If you have a question please submit an [issue](https://github.com/pauldendulk/Mapsui/issues) or post a question on stackoverflow with the 'mapsui' tag (I will get a notification).

## License 

LGPL
