# Projections

A geospatial projection involves converting coordinates from one coordinate system to another. If all your data is within a single coordinate system, projection is unnecessary. In the context of Mapsui, *projection* refers specifically to geospatial transformations, not to the conversion of spatial coordinates to pixel positions on a screen, even though both can be considered transformations.

## Some Background on Projections

Geospatial projections are complex, and explaining them can be challenging due to the varied backgrounds of Mapsui users. Some are experienced GIS professionals, while many are app developers needing a map for their application. Below are some basic concepts to help clarify this topic. I recommend watching [this video](https://www.youtube.com/watch?v=kIID5FDi2JQ) for an introduction to map projections.

## Spatial Reference System (CRS)

In geospatial contexts, coordinate systems are referred to by their Coordinate Reference System (CRS). In Mapsui, both the `Map` and the `IProvider` have a CRS field to specify their coordinate systems.

## Supported Coordinate Systems (CRSes)

By default, Mapsui supports projections between two main coordinate systems:

- [EPSG:4326](https://epsg.io/4326), also known as WGS84 or lat/lon, which is used by GPS.
- [EPSG:3857](https://epsg.io/3857), also known as SphericalMercator, WebMercator, or PseudoMercator, which is used by Google Maps and OpenStreetMap.

You can also create custom projections by implementing the `IProjection` interface and using a projection library like [ProjNet4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI).

## Parts of Mapsui Involved in Projections

- **Map**: The map itself, which is always in a specific coordinate system.
- **Layers**: These provide data to be displayed on the map and must return data in the map's coordinate system to avoid errors from overlapping different projections.
- **Providers**: Data sources for layers. If the data is in a different coordinate system, it can be converted using the `ProjectingProvider`.

## The Most Common Scenario

When using OpenStreetMap, the map is in SphericalMercator, but your geodata might be in lat/lon (e.g., a GPS track).

1. Set `Map.CRS` to "EPSG:3857" if using SphericalMercator.
2. Set `Provider.CRS` to "EPSG:4326" if your data is in lat/lon.
3. Wrap the provider in the `ProjectingProvider`. Refer to the code samples for `ProjectingProvider`.

The `ProjectingProvider` will handle the projection for you. Alternatively, you can manually project the data using `Mapsui.SphericalMercator.FromLonLat` and `ToLonLat` before adding it to a `MemoryLayer`, which eliminates the need to set CRSes.

## Remarks

- Mapsui does not support projecting images, such as raster tiles. The CRS fields are ignored for image projections.

## Sample

[!code-csharp[Main](../../Samples/Mapsui.Samples.Common/Maps/Projection/PointProjectionSample.cs#projectionsample "Projection Sample")]
