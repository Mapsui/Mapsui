# Frequently Asked Questions

## Why is all my data in a small area near the west coast of Africa
This is because the background data is in SphericalMercator (it is in the SphericalMercator 
coordinate system) and the foreground data is in WGS84 (latlon). Use 
SphericalMercator.FromLonLat to transform it.
