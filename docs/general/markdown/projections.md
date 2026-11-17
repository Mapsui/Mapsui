---
title: Projections
description: How coordinate system projections work in Mapsui, how to configure Map.CRS and DataSource.CRS, and background on geospatial transformations.
---

# Projections

A geospatial projection involves converting coordinates from one coordinate system to another. If all your data is within a single coordinate system, projection is unnecessary. In the context of Mapsui, *projection* refers specifically to geospatial transformations, not to the conversion of spatial coordinates to pixel positions on a screen, even though both can be considered transformations.

## Some Background on Projections

Geospatial projections are complex, and explaining them can be challenging due to the varied backgrounds of Mapsui users. Some are experienced GIS professionals, while many are app developers needing a map for their application. Watching [this video](https://www.youtube.com/watch?v=kIID5FDi2JQ) is a good introduction to map projections.

## Coordinate Reference System (CRS)

In geospatial contexts, coordinate systems are referred to by their Coordinate Reference System (CRS). In Mapsui, both the `Map` and the `IProvider` have a CRS field to specify their coordinate systems.

## Supported Coordinate Systems (CRSes)

By default, Mapsui supports projections between three coordinate systems:

- [EPSG:4326](https://epsg.io/4326), also known as WGS84 or lat/lon, which is used by GPS.
- [EPSG:3857](https://epsg.io/3857), also known as SphericalMercator, WebMercator, or PseudoMercator, which is used by Google Maps and OpenStreetMap.
- [EPSG:3395](https://epsg.io/3395), also known as World Mercator.

You can also support additional coordinate systems by implementing the `IProjection` interface (using a library like [ProjNet4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI)) and registering it globally:

```csharp
ProjectionDefaults.Projection = new MyCustomProjection();
```

The `ProjectingProvider` picks up `ProjectionDefaults.Projection` automatically.

## Parts of Mapsui Involved in Projections

- **Map**: The map itself, which is always in a specific coordinate system.
- **Layers**: These provide data to be displayed on the map and must return data in the map's coordinate system to avoid errors caused by mixing different coordinate systems.
- **Providers**: Data sources for layers. If the data is in a different coordinate system, it can be converted using the `ProjectingProvider`.

## The Most Common Scenario

When using OpenStreetMap, the map is in SphericalMercator, but your geodata might be in lat/lon (e.g., a GPS track).

1. Set `Map.CRS` to `"EPSG:3857"` if using SphericalMercator.
2. Set the inner provider's `CRS` to `"EPSG:4326"` to indicate the source coordinate system.
3. Wrap the provider in `ProjectingProvider` and set its `CRS` to `"EPSG:3857"` (matching the map) to indicate the target coordinate system:

```csharp
var memoryProvider = new MemoryProvider(features) { CRS = "EPSG:4326" };
var dataSource = new ProjectingProvider(memoryProvider) { CRS = "EPSG:3857" };
```

The `ProjectingProvider` will handle the projection for you. Alternatively, you can manually project the data using `Mapsui.Projections.SphericalMercator.FromLonLat` and `ToLonLat` before adding it to a `MemoryLayer`, in which case no CRS fields need to be set.

## Remarks

- Mapsui does not support projecting raster images such as tiles; CRS fields are ignored for those layers.

## Sample

This code comes from [PointProjectionSample.cs](https://github.com/Mapsui/Mapsui/blob/main/Samples/Mapsui.Samples.Common/Maps/Projection/PointProjectionSample.cs).

```csharp
public Task<Map> CreateMapAsync()
{
    // For Projections to work three things need to be set:
    // 1) The CRS on the Map to know what to project to.
    // 2) The CRS on the DataSource to know what to project from.
    // 3) The projection to project from the DataSource CRS to
    // the Map CRS.

    using var geometryLayer = CreateWorldCitiesLayer();
    var extent = geometryLayer.Extent!.Grow(10000);
    var map = new Map
    {
        CRS = "EPSG:3857", // The Map CRS needs to be set
        BackColor = Color.Gray
    };
    map.Layers.Add(OpenStreetMap.CreateTileLayer());
    map.Layers.Add(geometryLayer);
    map.Navigator.ZoomToBox(extent);

    map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Cities"));

    return Task.FromResult(map);
}

public static Layer CreateWorldCitiesLayer()
{
    var features = WorldCitiesFeaturesBuilder.CreateTop100Cities();

    var memoryProvider = new MemoryProvider(features)
    {
        CRS = "EPSG:4326" // The DataSource CRS needs to be set
    };

    var dataSource = new ProjectingProvider(memoryProvider)
    {
        CRS = "EPSG:3857"
    };

    return new Layer
    {
        DataSource = dataSource,
        Name = "Cities",
        Style = CreateCityStyle(),
    };
}

private static ImageStyle CreateCityStyle()
{
    var imageSource = "embedded://Mapsui.Samples.Common.Images.location.png";

    return new ImageStyle
    {
        Image = imageSource,
        Offset = new Offset { Y = 64 },
        SymbolScale = 0.25,
        Opacity = 0.5f
    };
}
```
