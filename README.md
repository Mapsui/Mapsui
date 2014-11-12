[![NuGet Status](http://img.shields.io/nuget/v/Mapsui.svg?style=flat)](https://www.nuget.org/packages/Mapsui/)

## Mapsui (pronounced map-su-wii) ##

Mapsui is a C# library for mapping applications.

- Intended to develop apps
- Designed to be fast and responsive (see [architecture](https://github.com/pauldendulk/Mapsui/wiki/Async-Fetching))
- All data fetching is on a background thread (disk, web, or database)
- Code is written to be used cross platform (using PCL or code sharing)
- Based on a modified version of SharpMap. 
- Uses BruTile to access tile services

###Get it from NuGet 
`
PM> Install-Package Mapsui
`

https://www.nuget.org/packages/Mapsui

## Platforms Supported ##

Mapsui, the core project, is a PCL with Profile336 which targets:

- .Net Framework 4.0.3 and higher
- Windows 8
- Windows Phone Silverlight 8
- Silverlight 5
- Windows Phone 8.1
- Xamarin.iOS
- Xamarin.Android

Per platform there are separate assemblies for the UI and for Rendering. Those are:

- Mapsui.UI.Xaml and Mapsui.Rendering.Xaml for WPF on .NET 4.0.3
- Mapsui.UI.Xaml-W8 and Mapsui.Rendering.Xaml-W8 for Windows Store apps
- Mapsui.UI.Xaml-WP8 and Mapsui.Rendering.Xaml-WP8 for Windows Phone 8
- Mapsui.UI.Xaml-SL and Mapsui.Rendering.Xaml-SL for Silverlight 5
- Mapsui.UI.Android and Mapsui.Rendering.Android for API Level 10 (v2.3)
- Mapsui.UI.iOS and Mapsui.Rendering.iOS

## Applications that use Mapsui ##

- [BergerView 2.0](http://www.geodan.com/organisation/news/latest-news/article/bergerview-20-for-stichting-incident-management/)
- [OutdoorMaps](http://www.outdoormaps.net/)
- [Phoenix](http://www.geodan.nl/producten/touch-table-software/phoenix/)
- [Bevoegd Gezag](http://apps.microsoft.com/windows/nl-nl/app/ceb52263-0e83-4bdd-a38a-bcab63295b9d)
- [EarthWatchers](http://earthwatchers.cloudapp.net/)

## Warnings ##

- There is limited documentation.
- Breaking changes will be introduced frequently. We change the API whenever we feel this is an improvement.
- We adopt new technologies relatively fast, and dropping support for older frameworks.
- Only use this project if you are willing to dig into the code.
- Although there is a general plan of where to go with this library there are not enough resources to go towards that goal in a systematic way. Functionality is added depending on what is needed in the projects it is used.

