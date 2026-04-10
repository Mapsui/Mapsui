---
applyTo: "Mapsui.Rendering.Skia/**,Mapsui.Experimental.Rendering.Skia/**"
---

# RichTextKit (Topten.RichTextKit) in Mapsui

RichTextKit provides text layout capabilities that SkiaSharp alone doesn't offer: Unicode line-breaking (UAX #14), bidirectional text (UAX #9), and font fallback for emoji and international scripts. Both renderers depend on it — it is not being replaced.

NuGet package: `Topten.RichTextKit`

---

## When to use RTK vs plain SkiaSharp

| Situation | Use |
|---|---|
| Latin text, single font, no word wrap | Plain `SKFont` + `canvas.DrawText()` |
| Word wrap (any script) | RTK `TextBlock` for line breaking |
| RTL / BiDi text | RTK `TextBlock.Paint()` |
| Emoji or font fallback | RTK `TextBlock.Paint()` |
| Mixed fonts/colors in one block | RTK `TextBlock` with multiple `AddText()` calls |

---

## TextBlock — core usage

```csharp
using Topten.RichTextKit;

var block = new TextBlock();
block.AddText("Hello world", new Style { FontFamily = "Arial", FontSize = 16, TextColor = SKColors.Black });
block.AddText(" bold", new Style { FontFamily = "Arial", FontSize = 16, FontWeight = 700 });

block.MaxWidth = 200f;               // enables UAX#14 word wrap; null = no wrap
block.MaxHeight = 100f;              // truncates at height; null = no limit
block.MaxLines = 3;                  // alternative to MaxHeight
block.Alignment = TextAlignment.Center;  // Left (default), Right, Center, Auto

block.Layout();  // idempotent; called lazily by property accessors too

float w = block.MeasuredWidth;
float h = block.MeasuredHeight;  // full font metrics: ascent + descent + leading
bool truncated = block.Truncated;

block.Paint(canvas, new SKPoint(x, y), new TextPaintOptions { Edging = SKFontEdging.Antialias });
```

Key notes:
- `MeasuredHeight` uses full font metrics — the same as `SKFont.Spacing`. Always use `font.Spacing` when computing line height manually so values stay consistent.
- Alignment is baked into glyph positions at layout time. If `MaxWidth` changes after a first layout pass, call `Layout()` again before painting.
- `TextAlignment.Auto` resolves to Left for LTR text, Right for RTL.
- Prefer `TextBlock.Clear()` + re-add over constructing a new instance for frequently-updated text — the internal arrays are reused, reducing GC pressure.

---

## Style — key properties

```csharp
new Style
{
    FontFamily    = "Arial",           // default "Arial"
    FontSize      = 16f,               // default 16
    FontWeight    = 400,               // 400=normal, 700=bold
    FontItalic    = false,
    TextColor     = SKColors.Black,
    HaloColor     = SKColor.Empty,     // glow/outline behind glyphs
    HaloWidth     = 0f,
    HaloBlur      = 0f,
    LineHeight    = 1.0f,              // line height multiplier
    LetterSpacing = 0f,
    FontVariant   = FontVariant.Normal,    // SuperScript, SubScript
    TextDirection = TextDirection.Auto,    // LTR, RTL, Auto
}
```

- `Style.Seal()` — makes immutable; safe to cache and share across blocks.
- `Style.Modify(...)` — returns a new style with selective overrides; does not mutate.

---

## FontMapper — binding RTK to a loaded typeface

By default RTK resolves typefaces via `SKTypeface.FromFamilyName()`. Override `FontMapper` to point it at an `SKTypeface` already loaded by Mapsui, so line-break measurements match what will actually be drawn:

```csharp
// MapsuiFontMapper — defined in SkiaTextLayoutHelper.cs
private sealed class MapsuiFontMapper(SKTypeface typeface) : FontMapper
{
    public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) => typeface;
}

var block = new TextBlock();
if (font.Typeface != null)
    block.FontMapper = new MapsuiFontMapper(font.Typeface);
block.AddText(text, new Style { FontFamily = font.Typeface?.FamilyName ?? "Arial", FontSize = font.Size });
```

Always set `FontMapper` when using a custom or loaded typeface — a mismatch causes line-break positions to differ from drawn output.

---

## Mapsui usage patterns

### Line-breaking only (label renderer)

Use RTK only for UAX#14 word wrap, then draw with plain SkiaSharp:

```csharp
var block = new TextBlock();
if (font.Typeface != null)
    block.FontMapper = new MapsuiFontMapper(font.Typeface);
block.AddText(line, new Style { FontFamily = font.Typeface?.FamilyName ?? "Arial", FontSize = font.Size });
block.MaxWidth = maxWidth;
block.Layout();

foreach (var textLine in block.Lines)
{
    var lineText = line.Substring(textLine.Start, textLine.Length).TrimEnd();
    result.Add(new Line { Value = lineText, Width = font.MeasureText(lineText, paint) });
}
// Draw with canvas.DrawText(), not RTK Paint()
```

### Full RTK rendering (callout, BiDi, emoji)

Use RTK for both layout and painting when BiDi or font fallback is needed. See `SkiaTextLayoutHelper.CreateTextBlock` / `PaintTextBlock`:

```csharp
var block = SkiaTextLayoutHelper.CreateTextBlock(text, font, alignment, color, maxWidth);
SkiaTextLayoutHelper.PaintTextBlock(canvas, block, x, y);
// PaintTextBlock calls: block.Paint(canvas, new SKPoint(x, y), new TextPaintOptions { Edging = SKFontEdging.Antialias })
```

### Double-layout for shared column width (callout title + subtitle)

Two blocks with different fonts must share a common column width for text alignment to work:

```csharp
// Pass 1: measure each independently
var titleBlock    = CreateTextBlock(title,    titleFont,    alignment, color, maxWidth);
var subtitleBlock = CreateTextBlock(subtitle, subtitleFont, alignment, color, maxWidth);
var width = Math.Max(titleBlock.MeasuredWidth, subtitleBlock.MeasuredWidth);

// Pass 2: re-layout at the shared width so alignment is correct
titleBlock.MaxWidth = subtitleBlock.MaxWidth = width;
titleBlock.Layout();
subtitleBlock.Layout();
```

---

## Accessing laid-out lines

```csharp
foreach (var line in block.Lines)
{
    int  start    = line.Start;                // code-point index into original string
    int  length   = line.Length;               // use text.Substring(start, length)
    float top     = line.YCoord;
    float baseline= line.YCoord + line.BaseLine;
    float height  = line.Height;               // ascent + descent + leading
    float width   = line.Width;                // excludes trailing whitespace
    var  nextLine = line.NextLine;             // null on last line
}
```

---

## Ellipsis and truncation

```csharp
block.MaxHeight = 60f;         // or MaxLines = 3
block.EllipsisEnabled = true;  // default — appends '…'
block.Layout();
bool wasTruncated = block.Truncated;
```

`block.AddEllipsis()` appends '…' to the last line post-layout without re-running the full layout — useful when you want ellipsis but not automatic truncation.

---

## StyleManager

`StyleManager` caches and deduplicates style instances and provides a Push/Pop stack with a fluent API — useful for building blocks with many mixed style runs:

```csharp
var sm = new StyleManager();
sm.DefaultStyle = new Style { FontFamily = "Arial", FontSize = 14 };

block.AddText("Normal ",    sm.CurrentStyle);
sm.Push();
block.AddText("Bold ",      sm.Bold(true));
block.AddText("+ italic ",  sm.FontItalic(true));
sm.Pop();
block.AddText("Normal.", sm.CurrentStyle);
```

Per-thread default: `StyleManager.Default`.

---

## Performance

- Never create `TextBlock` in the draw/paint loop. Cache results in `VectorCache` or `DrawableImageCache`.
- For dynamic text, prefer `block.Clear()` + re-add over creating a new instance — internal arrays are reused.
- `TextPaintOptions.IsAntialias` and `LcdRenderText` are `[Obsolete]` — use `Edging` instead.
- RTK uses HarfBuzzSharp for shaping — cost scales with text complexity, not byte length.

---

## Common pitfalls

| Pitfall | Fix |
|---|---|
| Center/right alignment renders as left | `MaxWidth` not set, or changed after layout — call `Layout()` again |
| Line height mismatch with manual drawing | Use `font.Spacing`, not tight glyph bounds from `font.MeasureText` |
| BiDi text renders in wrong order | Use `TextBlock.Paint()`, not `canvas.DrawText()` |
| Custom font ignored by RTK line breaker | Set `block.FontMapper` before `AddText()` |
| `Truncated` always false | `MaxHeight` or `MaxLines` must be set |
| Style changes after adding to block | Styles are read at layout time — mutate before `AddText()`, or use `Style.Seal()` |

---

## Key files

- [Mapsui.Experimental.Rendering.Skia/Extensions/SkiaTextLayoutHelper.cs](../../Mapsui.Experimental.Rendering.Skia/Extensions/SkiaTextLayoutHelper.cs) — RTK wrappers: `CreateTextBlock`, `PaintTextBlock`, `SplitByWordUnicode`, `MapsuiFontMapper`
- [Mapsui.Experimental.Rendering.Skia/SkiaStyles/LabelStyleRenderer.cs](../../Mapsui.Experimental.Rendering.Skia/SkiaStyles/LabelStyleRenderer.cs) — label rendering
- [Mapsui.Experimental.Rendering.Skia/SkiaStyles/CalloutStyleRenderer.cs](../../Mapsui.Experimental.Rendering.Skia/SkiaStyles/CalloutStyleRenderer.cs) — callout rendering (double-layout pattern)
- [Mapsui.Rendering.Skia/SkiaStyles/CalloutStyleRenderer.cs](../../Mapsui.Rendering.Skia/SkiaStyles/CalloutStyleRenderer.cs) — standard renderer callout (raw RTK API)
