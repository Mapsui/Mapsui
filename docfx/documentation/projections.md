# Projections

A geospatial projection is the transformation of coordinates in one coordinate system to another coordinate system. If all your data is in one coordinate system there is no need for projection. With a geospectial projection we do not mean transforming spatial coordinates to pixel positions on screen. To distinguish it from that kind of transformation we use the term *projection* instead of *transformation* for geospatial projections in Mapsui, although these words mean rougly the same. 

## Some background on projections 

The topic of geospatial projections is complicated. Users of Mapsui have a wide variety of backgrounds. Some are experienced GIS users that just need to know how this specific map component works. Many others are app developers that just happen to need a map for their current app. This makes it hard to explain things clear for everyone. Below are some basic concepts. I like [this](https://www.youtube.com/watch?v=kIID5FDi2JQ) video introduction to map projections.

## Spatial Reference System (CRS)

In geospatial there is a standard way to refer to a coordinate systems, the CRS (coordinate reference system). We will use the term CRS to refer to a specific coordinate system. In Mapsui the Map and the IProvider have a CRS field to indicate their coordinate systems.

## Supported coordinate systems (CRSes)

Out of the box Mapsui only supports the projection between two coordinate systems.
- [EPSG:4326](https://epsg.io/4326) or lat/lon, or WGS84. GPS coordinates are in lat/lon
- [EPSG:3857](https://epsg.io/3857) or SphericalMercator, or WebMercator, PseudoMercator. This is the coordinate system used in the maps of google and openstreetmap.

 It is however possible to create your own projection. You need to implement the IProjection interface. Within this implementation you need to use some other projection library. A recommended one is [ProjNet4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI).

## Parts of Mapsui involved in projections

- **Map**: There is one Map. It is inevitably in some kind of coordinate system.
- **Layers**: There are several layers that provider data. The layers always need to return data in the coordinate system that the map is using. If not, different projections
will be drawn on top of each other and things go wrong. 
- **Providers**: Some Layers have a DataSource (Provider). This DataSource could contain data in another coordinate system. This data can be converted to the map coordinate system using the ProjectingProvider. 

## The most common scenario

If you use OpenStreetMap the map is in SphericalMercator. Often you have geodata in lat/lon, say a GPS track.
1. The Map.CRS has to be set. If you use SphericalMercator set it to "EPSG:3857".
2. The Provider.CRS has to be set. If the data is in lat/lon set it to "EPSG:4326".
3. Wrap the Provider om the ProjectingProvider. Search the code samples for ProjectingProvider.

With this setup the ProjectingProvider will do the projection for you. Another option is to do the projection yourself. You can use Mapsui's SphericalMerator.FromLonLat/ToLonLat to project the data before you add them to a Memorylayer and no CRSes need to be set.

## Remarks

- Mapsui is not capable of projecting images. So no projection of tiles as rasters. No attempt will be made to project and the CRS fields will be ignored.

## Sample

[!code-csharp[Main](../../Samples/Mapsui.Samples.Common/Maps/Projection/PointProjectionSample.cs#projectionsample "Projection Sample")]
