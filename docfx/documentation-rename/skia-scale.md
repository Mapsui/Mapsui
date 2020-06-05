# Skia Scale

We have a separate page about this topic because skia scale has caused some confusion in the past and bugs as a consequence. 

## Some context: Device Independent Units 
Modern devices have a very high resolution. If something is drawn onto the canvas using raw pixels as coordinates the fonts would become tiny and unreadable and lines would become very thin. To correct for this a scale factor is used. Those scaled-up coordinates are called device independent units. Most of the time you deal with the device independent units.

### Coordinates in SkiaSharp
Most (all?) views in SkiaSharp use pixels as coordinates by default but for our purposes we need to use device independent units, so we need to correct for this. We do this by setting the scale of the SKCanvas. This needs to be done in the render loop because it is the only place where we have access. Also the size of the map needs to be adjusted otherwise we would draw outsize the screen.
