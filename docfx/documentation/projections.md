# Projections

Projections is a complicated topic. Users of Mapsui have a wide 
variety of backgrounds. Some are experienced GIS users that just need to
know how this specific map component works. Many others are app developers that 
just happen to need a map for their current app. This makes it hard to explain
things clear for everyone. Below are some basic concepts. I like [this](https://www.youtube.com/watch?v=kIID5FDi2JQ) video introduction to map projections.

## The most common scenario
Much point data is in a coordinates system called WGS84, or lat/long coordinates, or [EPSG:4326](https://epsg.io/4326). Most maps are in some another coordinate system which is better suited for display. The projection used in most online maps these days is SphericalMercator, or WebMercator, PseudoMercator or [EPSG:3857](https://epsg.io/3857). The OpenStreetMap tile layer that is used in many samples of Mapsui is also in SphericalMercator. If you use this map your coordinates also need to be in SphericalMercator. By default there is no automatic projection in Mapsui. The Mapsui helper methods for this transformation are SphericalMerator.FromLonLat/ToLonLat.

## Parts of Mapsui involved in projections
- **Map**: There is one Map. It is inevitably in some kind of projection.  
- **Layers**: There are several layers that provider data. This data
should be in the same projection as the Map. If not, different projections
will be drawn on top of each other and things go wrong. 
- **Providers**: Some Layers have a DataSource (Provider). This DataSource could contain
data in another projection. This data can be converted to the Map projection
but a few things need to be set.

## Configure Mapsui for automatic projections
Currently only the *Layer* layer type can be set up to do automatic projetions from DataSource to Map. Three things need to be configured for this:
1. The CRS on the Map to know what to project to.
2. The CRS on the DataSource to know what to project from.
3. The Transformation on the Map to calculate the projection from DataSource CRS to
the Map CRS.

## Support for projections
The out of the box Mapsui support for projections is limited. The
MinimalProjection class only projects between SphericalMercator 
(EPSG:3857) and WGS84 (EPSG:4326). It is however possible to create
your own Transformation. You need to implement the ITransformation
interface. Within this implementation you need to use some other 
projection library. A recommended one is [ProjNet4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI).

## The most common scenario
Most likely you will be fine if you use the same projection as Google 
and OpenStreetMap. This projection is called SphericalMercator. The 
official code from the OGC for this projection is EPSG:3857. If you use
the OpenStreetMap background layer you use EPSG:3857. Often you have 
GPS locations or points of interests (POIs) in WGS84 or EPGS:4326. These
points need to be transformed to EPSG:3857. There are two ways:
- Follow the configuration for projections mentioned above and in the 
ProjectionSample.cs.
- Use SphericalMercator.FromLonLat to do the transformation manually.

## Remarks
- A Layer has a CRS field. This field is used by Mapsui to set it to the 
Map projection. It should not be set by the user. This is could be confusing.
- Mapsui is not capable of transforming images. So no transformation of tiles 
as rasters. No attempt will be made to transform and the CRS fields will be 
ignored.

