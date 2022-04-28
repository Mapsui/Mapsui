# Device Independent Units

Modern devices have a very high resolution. If something is drawn onto the canvas using raw pixels as coordinates the fonts would become tiny and unreadable and lines would become very thin. To correct for this a scale factor is used. Those scaled-up coordinates are called device independent units. Most of the time users deal with the device independent units. In some context they are also called device independent pixels, or dip, or dp.

### Device Independent Units in SkiaSharp

The scale in skia has caused some confusion in the past and bugs as a consequence. So here some extra information on this topic. This is mainly targetted at contributors, users of the Mapsui nugets do not need to know about this.

Most (all?) views in SkiaSharp use pixels as coordinates by default but for our purposes we need to use device independent units, so we need to correct for this. We do this by setting the scale of skia's SKCanvas. This needs to be done in the render loop because it is the only place where we have access to the SKCanvas. Also the size of the map needs to be adjusted at that point, otherwise we would draw outsize the screen.
