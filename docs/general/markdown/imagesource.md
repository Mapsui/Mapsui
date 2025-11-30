# Images

In Mapsui v5 the Image class was added which simplifies the process of working with images. Previously, in v4, you had to load the image in your code, register it, and then assign the image ID returned from the registration to the symbol. Now all you have to do is assign the path to the image to the Source field of the Image class.

### Supported Path Schemes for the Source

Mapsui supports five types of schemes for specifying image paths: 

- **http(s)://**, for pointing images on a remote server.
- **file://**, for pointing to images on the local file system.
- **embedded://**, for pointing to images in the dotnet embedded resources.
- **svg-content://**, for strings that contain the SVG xml itself.
- **base64-content://**, for strings containing base64 encoded images.

Here are some examples:

```csharp
myStyle.Image.Source = "https://mapsui.com/images/logo.svg";
myStyle.Image.Source = $"file://{Environment.SpecialFolder.LocalApplicationData}/example.png"
myStyle.Image.Source = "embedded://Mapsui.Resources.Images.Pin.svg"
myStyle.Image.Source = "svg-content://<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"36\" height=\"56\"><path d=\"M18 .34C8.325.34.5 8.168.5 17.81c0 3.339.962 6.441 2.594 9.094H3l7.82 15.117L18 55.903l7.187-13.895L33 26.903h-.063c1.632-2.653 2.594-5.755 2.594-9.094C35.531 8.169 27.675.34 18 .34zm0 9.438a6.5 6.5 0 1 1 0 13 6.5 6.5 0 0 1 0-13z\" fill=\"#ffffff\" stroke=\"#000000\"/></svg>")]
myStyle.Image.Source = "base64-content://PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzNiIgaGVpZ2h0PSI1NiI+PHBhdGggZD0iTTE4IC4zNEM4LjMyNS4zNC41IDguMTY4LjUgMTcuODFjMCAzLjMzOS45NjIgNi40NDEgMi41OTQgOS4wOTRIM2w3LjgyIDE1LjExN0wxOCA1NS45MDNsNy4xODctMTMuODk1TDMzIDI2LjkwM2gtLjA2M2MxLjYzMi0yLjY1MyAyLjU5NC01Ljc1NSAyLjU5NC05LjA5NEMzNS41MzEgOC4xNjkgMjcuNjc1LjM0IDE4IC4zNHptMCA5LjQzOGE2LjUgNi41IDAgMSAxIDAgMTMgNi41IDYuNSAwIDAgMSAwLTEzeiIgZmlsbD0iI2ZmZmZmZiIgc3Ryb2tlPSIjMDAwMDAwIi8+PC9zdmc+")]
myStyle.Image.Source = "base64-content://iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAIAAAAC64paAAAACXBIWXMAAC4jAAAuIwF4pT92AAAAcUlEQVQ4y+VUyw7AIAgrxgtf4v9/HZ5kF90M6JK9siVruGGlNFVSVZxFwAXMyURrlZwPTy4i2F3qIdmfJsfNW4/mVmAetqI/alV5w9uku3buUlGzIQJAU7ItS1a11cmraTHdf4dkeDEzAAJmL4te+0kWaRI0VGH3VHwAAAAASUVORK5CYII="
```

### Supported Image Source Types 

An Image Source can point to or contain an SVG (supported via [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia)) or a bitmap type, which can be PNG, WEBP, JPEG, and any other format [supported by SkiaSharp](https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skencodedimageformat?view=skiasharp-2.88#fields).

### Image usage

The following Mapsui classes have an Image field:

- ImageStyle
- CalloutStyle
- Brush
- ImageButtonWidget

### BitmapRegion

If the Source refers to a bitmap type (so PNG or webp, not SVG), you can specify a sub-region of the bitmap to use. This is useful for utilizing a smaller part of a bitmap or working with a meta image (or atlas) that contains multiple smaller images. 

### Custom SVG colors

If the Source refers to an SVG, you can override the built-in colors of the stroke and fill with:

- SvgFillColor
- SvgStrokeColor

This feature offers great flexibility, allowing you to, for instance, indicate different types of vehicles or different states of a single vehicle. 

!!! warning

    Note that a new instance of the SVG object needs to be stored in memory for every different color. So you need to keep some restraint on the number of colors used. An example where this could cause trouble is if you use a float value in a calculation to determine the color, for instance if you let the speed of a vehicle determine the color. To circumvent this you could use categories. Many SVG object instances can also impact performance. If a single color suffices you could use BlendColorMode as an alternative to SvgFillColor/SvgStrokeColor. 
