# ImageSource

In Mapsui v5, image sources have been introduced, simplifying the process of assigning images to symbols. Previously, in v4, you had to load the image in your code, register it, and then assign the image ID returned from the registration to the symbol. With v5, you can directly assign the image path to the symbol.

### Supported Path Schemes
Mapsui supports three types of schemes for specifying image paths: 'http(s)', 'file', and 'embedded' (for pointing to embedded resources). Here are some examples:

```csharp
myStyle.ImageSource = "https://mapsui.com/images/logo.svg";
myStyle.ImageSource = $"file://{Environment.SpecialFolder.LocalApplicationData}/example.png"
myStyle.ImageSource = "embedded://Mapsui.Resources.Images.Pin.svg"
```

### Image Types 
An ImageSource can point to an SVG (supported via [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia)) or a bitmap type, which can be PNG, WEBP, JPEG, and any other formats [supported by SkiaSharp](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skencodedimageformat?view=skiasharp-2.88#fields).

### Types with an ImageSource
In Mapsui 5.0.0-beta.2 the following types support the ImageSource path:

- SymbolStyle
- CalloutStyle
- Brush
- ImageButtonWidget

### BitmapRegion for Bitmap Types
For bitmap types, you can specify a sub-region of the bitmap to use. This is useful for utilizing a smaller part of a bitmap or working with a meta image (or atlas) that contains multiple smaller images. 

In Mapsui 5.0.0-beta.2 the following types support the BitmapRegion:

- SymbolStyle 
- Brush

### Custom SVG colors
When using an SVG, you can override the built-in colors of the stroke and fill with:

- SvgFillColor
- SvgStrokeColor

This feature offers great flexibility, allowing you to, for instance, indicate different types of vehicles or different states of a single vehicle. 

In Mapsui 5.0.0-beta.2 only the SymbolStyle supports SvgFillColor and SvgStrokeColor.

!!! warning

    Note that a new instance of the SVG object needs to be stored in memory for every different color. So you need to keep some restraint on the number of colors used. An example where this could cause trouble is if you use a float value in a calculation to determine the color, for instance if you let the speed of a vehicle determine the color. To circumvent this you could use categories. Many SVG object instances can also impact performance. If a single color suffices you could use BlendColorMode as an alternative to SvgFillColor/SvgStrokeColor. 
