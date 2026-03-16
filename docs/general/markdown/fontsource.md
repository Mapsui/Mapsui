# Custom Fonts

In the experimental renderer, the `FontSource` property on the `Font` class lets you load a custom font from an external source. This works the same way as `ImageSource` for images: assign a URI string to `Font.FontSource` and Mapsui fetches and caches the font bytes automatically.

```csharp
myLabelStyle.Font = new Font
{
    FontFamily = "OpenSans",  // used as display name; actual typeface comes from FontSource
    Size = 16,
    FontSource = "embedded://MyApp.Resources.Fonts.OpenSans-Regular.ttf"
};
```

The implicit `string` → `FontSource` conversion means you can assign a plain string directly:

```csharp
myFont.FontSource = "embedded://MyApp.Resources.Fonts.OpenSans-Regular.ttf";
```

### Supported URI Schemes

FontSource supports four URI schemes:

- **embedded://**, for fonts bundled as .NET embedded resources.
- **file://**, for fonts on the local file system.
- **http://** and **https://**, for fonts hosted on a remote server.

!!! note

    `svg-content://` and `base64-content://` are not supported (unlike `ImageSource`). Using either will throw an `ArgumentException` when assigning the `FontSource`. SVG is not a font format. Base64-encoded font bytes would technically decode correctly at the fetcher level, but embedding an entire font as a base64 string is not a practical use case and is intentionally excluded.

### Embedded resource example

Mark the font file as an embedded resource in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\Fonts\OpenSans-Regular.ttf" />
</ItemGroup>
```

Then reference it using the assembly-qualified dotted resource name:

```csharp
myFont.FontSource = "embedded://MyApp.Resources.Fonts.OpenSans-Regular.ttf";
```

The resource name follows the standard .NET convention: `<RootNamespace>.<FolderPath>.<FileName>` with path separators replaced by dots.

### File system example

```csharp
myFont.FontSource = $"file://{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "myapp", "fonts", "OpenSans-Regular.ttf")}";
```

### Remote example

```csharp
myFont.FontSource = "https://example.com/fonts/OpenSans-Regular.ttf";
```

### Which styles use Font

The following Mapsui style and widget classes have a `Font` property that supports `FontSource`:

- `LabelStyle` (via `Font`)
- `CalloutStyle` (via `TitleFont` and `SubtitleFont`)
- `ScaleBarWidget` (via `Font`)

!!! warning

    `FontSource` is currently only supported in the **experimental renderer** (`Mapsui.Experimental.Rendering.Skia`). The standard renderer ignores `FontSource` and falls back to `FontFamily` name lookup.

### Caching

Font bytes are fetched once per unique source URI and cached for the lifetime of the map. The cache is stored in `RenderService.FontSourceCache`. There is no need to pre-register fonts — declaring a `FontSource` on any `Font` object is enough for Mapsui to schedule the fetch automatically on the next render cycle.
