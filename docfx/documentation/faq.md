# Frequently Asked Questions

## Why is all my data in a small area near the west coast of Africa
This is because the background data used is in the SphericalMercator coordinate 
system and the data of the layer on top is in WGS84 (latlon). Use 
SphericalMercator.FromLonLat to transform it.
