## Development constraints

When developing for Mapsui we take into account the following constraints.

### Not limited to certain coordinate system
Mapsui's Map can be in any coordinate system. If you do not specify a coordinate system in the Map and Layers it assumes they are in the same coordinate system (whatever they are). In this case it only transforms these unspecified 'world-coordinates' to 'screen-coordinates' and nothing more. It is also possible to setup a coordinate transformation system using Map.CRS, DataSource.CRS and Map.Transformation. See [projections](projections.md).

### Full implementation of the feature matrix
These are some of the feature dimensions:
- Renderers: WPF and Skia
- Geometries: Point, LineString, Polygon etc.
- Operations on Geometries: Distance, Contains.
- Coordinate projection support
- Style: fill color, line color, line cap, symbol opacity, symbol scale 

If we choose to support a feature each 'cell' of the multi dimensional matrix should be supported. No surprises for the user.

Currently there are holes in the matrix on some point (like differences between WPF and Skia). The current focus is to fill these holes.

If this support does not seem attainable (is that proper English?) but does seem very useful we should look for ways to make Mapsui extendable 

### Write clear, simple and little code
Maintenance is enemy that can bring a project like this to a halt. We should look for ways to implement the functionality with the simplest possible code.

