# Projections

A geospatial projection is the transformation of coordinates in one coordinate system to another coordinate system. If all your data is in one coordinate system there is no need for projetion. With a geospectial projection we do not mean transforming spatial coordinates to pixel positions on screen. To distinguish it from that kind of transformation we use the term projection instead of transformation for geospatial projections in Mapsui, although these words mean rougly the same. 

## Some background on projections 

Projections is a complicated topic. Users of Mapsui have a wide variety of backgrounds. Some are experienced GIS users that just need to know how this specific map component works. Many others are app developers that just happen to need a map for their current app. This makes it hard to explain things clear for everyone. Below are some basic concepts. I like [this](https://www.youtube.com/watch?v=kIID5FDi2JQ) video introduction to map projections.

## Spatial Reference System (CRS)

In geospatial there is a standard way to refer to a coordinate systems, the CRS (coordinate reference system). We will use the term CRS to refer to a specific coordinate system. In Mapsui the Map and the IProvider have a CRS field to indicate their coordinate systems.

## The most common scenario

Most point data is in a coordinates system called WGS84, or lat/long coordinates, or [EPSG:4326](https://epsg.io/4326). Most maps are in some other coordinate system which is better suited to display a certain region. The coordinate system used in most online maps these days is SphericalMercator, or WebMercator, PseudoMercator or [EPSG:3857](https://epsg.io/3857). The OpenStreetMap tile layer that is used in many samples of Mapsui is also in SphericalMercator. If you use this layer in you map your coordinates also need to be in SphericalMercator. By default there is no automatic projection in Mapsui. If transformation is needed you could either use the ProjectingProvider or transform your coordinates before you store them in your data source.  Mapsui has a helper methods for the projection from lat/lon (EPSG:4326) to spherical mercator (EPSG:3857): SphericalMerator.FromLonLat/ToLonLat.

## Parts of Mapsui involved in projections

- **Map**: There is one Map. It is inevitably in some kind of coordinate system. The Map.CRS field indicates the coordinate system the map is using.
- **Layers**: There are several layers that provider data. The layers always need to return data in the coordinate system that the map is using. If not, different projections
will be drawn on top of each other and things go wrong. 
- **Providers**: Some Layers have a DataSource (Provider). This DataSource could contain data in another coordinate system. This data can be converted to the map coordinate system using the ProjectingProvider. This is what needs to done:
1. The Map.CRS has to be set. If you use OpenStreetMap set it to "EPSG:3857".
2. The Provider.CRS has to be set. If the data is in lat/lon set it to "EPSG:4326".
3. Wrap the Provider om the ProjectingProvider. Search the code samples for ProjectingProvider.

## Support for projections

Out of the box Mapsui only supports projection between SphericalMercator (EPSG:3857) and WGS84 (EPSG:4326). It is however possible to create your own projection. You need to implement the IProjection interface. Within this implementation you need to use some other projection library. A recommended one is [ProjNet4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI).

## The most common scenario

Most likely you will be fine if you use the coordinate system that Google Maps and and OpenStreetMap uses. This projection is called SphericalMercator. The official code from the OGC for this projection is EPSG:3857. If you use the OpenStreetMap background layer you use EPSG:3857. Often you have GPS locations or points of interests (POIs) in WGS84 or EPGS:4326. These points need to be transformed to EPSG:3857. There are two ways: 
- Follow the configuration for projections mentioned above and in the 
ProjectionSample.cs.
- Use SphericalMercator.FromLonLat to do the transformation manually.

## Remarks

- A Layer has a CRS field. This field is used by Mapsui to set it to the 
Map projection. It should not be set by the user. This is could be confusing.
- Mapsui is not capable of transforming images. So no transformation of tiles 
as rasters. No attempt will be made to transform and the CRS fields will be 
ignored.

