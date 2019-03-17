# Frequently Asked Questions

### Why is all my data in a small area near the west coast of Africa?
This is because the background data is in SphericalMercator (it is in the SphericalMercator 
coordinate system) and the foreground data is in WGS84 (latlon). Use 
SphericalMercator.FromLonLat to transform it.
Note: There can be many other forms of mixing up coordinate systems, but this is the most common.

### Why does NavigateTo zoom into an area near the west coast of Africa?
This is because the coordinates you pass to NavigateTo are in WGS84 whereas the
background data is in SphericalMercator. Use SphericalMercator.FromLonLat to transform 
the NavigateTo arguments to SphericalMercator.
Note: There can be many other forms of mixing up coordinate systems, but this is the most common.


