# Projections

A geospatial projection involves converting coordinates from one coordinate system to another. If all your data is within a single coordinate system, projection is unnecessary. In the context of Mapsui, *projection* refers specifically to geospatial transformations, not to the conversion of spatial coordinates to pixel positions on a screen, even though both can be considered transformations.

## Some Background on Projections

Geospatial projections are complex, and explaining them can be challenging due to the varied backgrounds of Mapsui users. Some are experienced GIS professionals, while many are app developers needing a map for their application. Watching [this video](https://www.youtube.com/watch?v=kIID5FDi2JQ) is a good introduction to map projections.

## Spatial Reference System (CRS)

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

The `ProjectingProvider` will handle the projection for you. Alternatively, you can manually project the data using `Mapsui.SphericalMercator.FromLonLat` and `ToLonLat` before adding it to a `MemoryLayer`, in which case no CRS fields need to be set.

## Remarks

- Mapsui does not support projecting raster images such as tiles; CRS fields are ignored for those layers.

## Sample

[!code-csharp[Main](../../Samples/Mapsui.Samples.Common/Maps/Projection/PointProjectionSample.cs#projectionsample "Projection Sample")]
