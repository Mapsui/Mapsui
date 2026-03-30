# VectorStyle

`VectorStyle` is the standard style for rendering vector geometries. It has three properties — `Fill`, `Line`, and `Outline` — but which ones are relevant depends on the geometry type being drawn. `Point`, `LineString`, and `Polygon` are NetTopologySuite (NTS) types from the `Mapsui.Nts` package. Mapsui's own `MPoint` renders identically to the NTS `Point`.

### Point and MPoint

Points are rendered as a circle. Use `Fill` to set the interior colour and `Outline` for the circle border.

```csharp
var style = new VectorStyle
{
    Fill = new Brush(Color.CornflowerBlue),
    Outline = new Pen(Color.DarkBlue, width: 2)
};
```

### LineString

Lines use `Line` as the main stroke. `Outline` is drawn wider behind the line, which creates a border or halo effect. `Fill` has no effect.

```csharp
var style = new VectorStyle
{
    Line = new Pen(Color.Violet, width: 3),
    Outline = new Pen(Color.Black, width: 5) // drawn behind Line, creates a border
};
```

### Polygon

Polygons use `Fill` for the interior and `Outline` for the border stroke. `Line` has no effect.

```csharp
var style = new VectorStyle
{
    Fill = new Brush(Color.CornflowerBlue),
    Outline = new Pen(Color.DarkBlue, width: 2)
};
```

## FillStyle patterns

`Brush.FillStyle` controls how the fill area is rendered. The available values are:

| Value | Description |
|---|---|
| `Solid` | Filled with a single colour (default) |
| `Hollow` | No fill — transparent interior |
| `Dotted` | Dot pattern |
| `Cross` | Horizontal and vertical lines crossing |
| `DiagonalCross` | Diagonal lines crossing |
| `BackwardDiagonal` | Lines from upper-right to lower-left |
| `ForwardDiagonal` | Lines from upper-left to lower-right |
| `Horizontal` | Horizontal lines |
| `Vertical` | Vertical lines |
| `Bitmap` | Tiled bitmap image |
| `BitmapRotated` | Tiled bitmap image, rotated |
| `Svg` | Tiled SVG image |

```csharp
var style = new VectorStyle
{
    Fill = new Brush { Color = Color.Red, FillStyle = FillStyle.Cross }
};
```

## Combining a pattern fill with a background colour

`VectorStyle.Fill` holds a single `Brush`, so a pattern and a solid background cannot both be expressed in one style. The solution is to stack two styles — a solid background rendered first, and the pattern rendered on top.

### Same background for all features

Set a solid `VectorStyle` on the layer, and add the patterned style to individual features:

```csharp
var layer = new MemoryLayer
{
    Style = new VectorStyle { Fill = new Brush(Color.CornflowerBlue) } // background for all features
};

var feature = new GeometryFeature { Geometry = myPolygon };
feature.Styles.Add(new VectorStyle
{
    Fill = new Brush { Color = Color.Red, FillStyle = FillStyle.Cross }
});
```

### Different background per feature

Add a `StyleCollection` to the feature with the solid style first, then the patterned style:

```csharp
feature.Styles.Add(new StyleCollection
{
    Styles =
    [
        new VectorStyle { Fill = new Brush(Color.CornflowerBlue) },
        new VectorStyle { Fill = new Brush { Color = Color.Red, FillStyle = FillStyle.Cross } }
    ]
});
```

## Hit detection on patterned and hollow fills

Mapsui determines a hit by checking for a non-transparent pixel at the tapped location. A patterned or `FillStyle.Hollow` fill is transparent between the strokes, so tapping between the pattern lines registers as a miss.

To make the whole polygon area hittable, add a nearly-transparent solid background using one of the layering approaches above. An alpha value of 1 is enough:

```csharp
new VectorStyle { Fill = new Brush(new Color(0, 0, 0, 1)) } // invisible but hittable
```
