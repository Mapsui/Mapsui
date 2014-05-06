## Mapsui (pronounced map-su-wii) ##

Mapsui is a C# library for mapping applications.

- Intended to develop apps
- It is designed to be fast and responsive (see architecture)
- All data fetching is on a background thread (disk, web, or database)
- Code is written to be used cross platform (using PCL or code sharing)
- Based on a modified version of SharpMap. 
- Uses BruTile to access tile services

## Applications that use Mapsui ##

- BergerView 2.0 http://www.geodan.com/organisation/news/latest-news/article/bergerview-20-for-stichting-incident-management/
- OutdoorMaps http://www.outdoormaps.net/
- Phoenix http://www.geodan.nl/producten/touch-table-software/phoenix/
- Bevoegd Gezag http://apps.microsoft.com/windows/nl-nl/app/ceb52263-0e83-4bdd-a38a-bcab63295b9d
- EarthWatchers http://earthwatchers.cloudapp.net/

## Platforms Supported ##

Mapsui, the core project, is compiled as Portable Class Library (Profile 147) for:

- .NET framework 4.0.3 and higher
- .NET for Windows Store apps
- Windows Phone 8 and higher
- Silverlight 5 and higher
- Xamarin.Android
- Xamarin.iOS

Per platform there are separate assemblies for the UI and for Rendering. Those are:

- Mapsui.UI.Xaml and Mapsui.Rendering.Xaml for WPF on .NET 4.0.3
- Mapsui.UI.Xaml-SL and Mapsui.Rendering.Xaml-SL for Silverlight 5
- Mapsui.UI.Xaml-WP8 and Mapsui.Rendering.Xaml-WP8 for Windows Phone 8
- Mapsui.UI.Xaml-W8 and Mapsui.Rendering.Xaml-W8 for Windows 8
- Mapsui.UI.Android and Mapsui.Rendering.Android for API Level 10 (v2.3)
- Mapsui.UI.iOS and Mapsui.Rendering.iOS

## Warnings ##

- There is limited documentation.
- Breaking changes will be introduced frequently. We change the API whenever we feel this is an improvement.
- We adopt new technologies relatively fast, and dropping support for older frameworks.
- Only use this project if you are willing to dig into the code.
- Although I do have a general plan of where to go with this library I do not have the resources to go towards that goal in a systematic way. I add functionality depending on what is needed in the projects I work on.

