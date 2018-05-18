# Skia Scale

We have a separate page about this topic because skia scale has caused some confusion in the past and bugs as a consequence. 

## Why skia scale?
Modern devices have a very high resolution. If we would draw onto the skia canvas using device pixels as coordinates the fonts would become tiny and unreadable and lines would become very thin. To correct for this we use a scale factor. We get the scale factor from the operating system.

## How does it work
The skia surface has it's own scale. We set this to an appropriate scale factor. As a result the coordinates of the skia surface are different than that of it's parent container. We have to correct for this on a number of points, like in rendering, requesting map info and manipulation.

## What do we do
We take this into account on a number of places in our code:

- We determine the skia scale while initializing (or when have switched to the skia renderer)
- We set the skia scale on the surface. This needs to be done in the render loop because this is the only place where we have access to the surface
- We set the viewport width and height to the skia width and height. We do this when:
  - initializing (TryInitializeViewport) and
  - whenever the parent's size changes.
- We pass the skia scale along if we request map info. Why not correct for scale before the call? Because we want to return the original click position as part of the response.
- When requesting the touch positions when manipulating the map.