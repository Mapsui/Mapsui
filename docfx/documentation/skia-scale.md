# Skia Scale

We have a separate page about this topic because skia scale has caused some confusion in the past and bugs as a consequence. 

## Some context: Device Independent Units 
Modern devices have a very high resolution. If something is drawn onto the canvas using raw pixels as coordinates the fonts would become tiny and unreadable and lines would become very thin. To correct for this a scale factor is used. Those scaled-up coordinates are called device independent units. Most of the time you deal with the device independent units.

### Coordinates in SkiaSharp
Most (all?) views in SkiaSharp use pixels as coordinates by default but for our purposes we need to use device independent units, so we need to correct for this. There are two ways this can be done:
- Set the skia view's IgnorePixelScaling.
- Call the skia view's Scale factor with the appropriate scale. For this you need to request the scale factor from the system.

## What do we do?
We take skia scale into account on a number of places in our code:

On WPF and UWP:
- We set IgnorePixelScaling.

On iOS:
- The GL view we use has no IgnorePixelScaling unfortunately.
- We determine the skia scale while initializing (or when switching from wpf to skia rendering)ent's size changes.
- We set the skia scale on the skia surface. This needs to be done in the render loop because this is the only place where we have access to the surface.

On Android:
- We determine the density (pixels per device independent units) while initializing.
- We initialze the viewport width and height to the skia width and height (on TryInitializeViewport).
- We set the viewport width and height to the skia width and height whenever the containing parent's size changes.
- We set the skia scale on the skia surface. This needs to be done in the render loop because this is the only place where we have access to the surface.
- We pass the skia scale along if we request map info. Why not directly correct the position for scale at the call? Because we want to return the original click position as part of the response. This position can be usefull and we don't want to user to correct for skia scale (in fact we don't want the user to have access to it).
- When requesting the touch positions when manipulating the map.
