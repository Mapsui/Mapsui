# Projections

Projections is a complicated topic. Users of Mapsui have a wide 
variaty of backgrounds. Some are experienced GIS users that just need to
know how this specific map component works. Many others are app developers that 
just happen to need a map for their current app. This makes it hard to explain
things clear for everyone. Here are some basic concepts.

## Some elements involved
- **Map**: There is one Map. It is inevitably in some kind of projection.  
- **Layers**: There are several layers that provider data. This data
should be in the same projection as the Map. If not, different projections
will be drawn on top of each other and things go wrong. 
- **Providers**: Some Layers have a DataSource (Provider). This DataSource could contain
data in another projection. This data can be converted to the Map projection
but a few things need to be set.

## Configure Mapsui for projections
Three things need to be set to allow projection from DataSource to Map
1. The CRS on the Map to know what to project to.
2. The CRS on the DataSource to know what to project from.
3. The Transformsion on the Map to transform from the DataSource CRS to
the Map CRS.

## Support for projections
Out of the box's Mapsui's support for projections is limited. The
MinimalProjection class only projects between SphericalMercator 
(EPSG:3857) and WGS83 (EPSG:4326). It is however possible to create
your own Transformation. You need to implement the ITransformation
interface. Within this implementation you need to use some other 
projection library. A recommende one is [ProjNet4GeoAPI](https://github.com/NetTopologySuite/ProjNet4GeoAPI).

## The most common scenario
Most likely you will be fine if you use the same projection as Google 
and OpenStreetMap. This projection is called SphericalMercator. The 
official code from the OGC for this projection is EPSG:3857. If you use
the OpenStreetMap background layer you use EPSG:3857. Often you have 
GPS locations or points of interests (POIs) in WGS84 or EPGS:4326. These
points need to be transformed to EPSG:3857. There are two ways:
- 1. Follow the configuration for projections mentioned above and in the 
ProjectionSample.cs.
- 2. Use SphericalMercator.FromLonLat to do the transformation manually.

## Remarks
A Layer has a CRS field. This field is used by Mapsui to set it to the 
Map projection. It should not be set by the user. This is could be confusing.

