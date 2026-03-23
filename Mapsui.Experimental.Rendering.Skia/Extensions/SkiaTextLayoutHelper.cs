using Mapsui.Widgets;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Topten.RichTextKit;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

public static class SkiaTextLayoutHelper
{
    public class Line
    {
        public string? Value { get; set; }
        public float Width { get; set; }
        public float Baseline { get; set; }
        public float BoundsLeft { get; set; }
        public float BoundsWidth { get; set; }
    }

    public static Line[] SplitLines(string? text, SKFont font, SKPaint paint, float maxWidth, string splitCharacter)
    {
        if (text == null)
            return [];

        var lines = text.Split('\n');

        return lines.SelectMany(line =>
        {
            if (splitCharacter == string.Empty)
                return SplitByCharacter(line, font, paint, maxWidth);

            return SplitByWordUnicode(line, font, paint, maxWidth);
        }).ToArray();
    }

    private static List<Line> SplitByCharacter(string line, SKFont font, SKPaint paint, float maxWidth)
    {
        var result = new List<Line>();
        var lineResult = new StringBuilder();
        float width = 0;

        foreach (var ch in line)
        {
            var charStr = ch.ToString();
            var charWidth = font.MeasureText(charStr, paint);

            if (width + charWidth > maxWidth && lineResult.Length > 0)
            {
                result.Add(new Line { Value = lineResult.ToString(), Width = width });
                lineResult = new StringBuilder();
                width = 0;
            }

            lineResult.Append(charStr);
            width += charWidth;
        }

        result.Add(new Line { Value = lineResult.ToString(), Width = width });
        return result;
    }

    private static List<Line> SplitByWordUnicode(string line, SKFont font, SKPaint paint, float maxWidth)
    {
        var result = new List<Line>();
        if (line.Length == 0)
        {
            result.Add(new Line { Value = "", Width = 0 });
            return result;
        }

        // Use RichTextKit's TextBlock for UAX#14-compliant line breaking.
        // TextBlock.Layout() uses the Unicode Line Break Algorithm internally.
        // Set a custom FontMapper so RTK measures line-break positions using the
        // same typeface that will be used for drawing (important for custom fonts).
        var textBlock = new TextBlock();
        if (font.Typeface != null)
            textBlock.FontMapper = new MapsuiFontMapper(font.Typeface);
        textBlock.AddText(line, new Style
        {
            FontFamily = font.Typeface?.FamilyName ?? "Arial",
            FontSize = font.Size,
        });
        textBlock.MaxWidth = maxWidth;
        textBlock.Layout();

        foreach (var textLine in textBlock.Lines)
        {
            var lineText = line.Substring(textLine.Start, textLine.Length).TrimEnd();
            var lineWidth = font.MeasureText(lineText, paint);
            result.Add(new Line { Value = lineText, Width = lineWidth });
        }

        if (result.Count == 0)
            result.Add(new Line { Value = "", Width = 0 });

        return result;
    }

    // Redirects all RTK font lookups to a specific pre-loaded SKTypeface so that
    // line-break measurements match the typeface that will actually be drawn.
    private sealed class MapsuiFontMapper(SKTypeface typeface) : FontMapper
    {
        public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) => typeface;
    }

    /// <summary>
    /// Layout text with word wrap and alignment, measuring the result size.
    /// Returns the lines and total size of the text block.
    /// </summary>
    public static (Line[] Lines, float Width, float Height) LayoutText(
        string? text, SKFont font, SKPaint paint, float maxWidth, Alignment alignment)
    {
        if (string.IsNullOrEmpty(text))
            return ([], 0, 0);

        font.MeasureText(text, out var rect, paint);
        var baseline = -rect.Top;
        var lineSpacing = font.Spacing;

        Line[] lines;
        if (maxWidth > 0 && (rect.Right - rect.Left) > maxWidth)
            lines = SplitLines(text, font, paint, maxWidth, " ");
        else
            lines = [new Line { Value = text, Width = font.MeasureText(text, paint) }];

        var width = 0f;
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i].Baseline = baseline + lineSpacing * i;
            if (lines[i].Width > width)
                width = lines[i].Width;
        }

        // Use font.Spacing (= ascent + descent + leading) as the height per line, matching
        // RichTextKit's (the previous implementation) MeasuredHeight behavior which also includes leading. 
        // Without leading, the title block would be shorter than RTK's, causing the subtitle to sit too close.
        var height = lineSpacing * lines.Length;
        return (lines, width, height);
    }

    /// <summary>
    /// Draw text lines onto a canvas with the given alignment.
    /// </summary>
    public static void DrawTextBlock(SKCanvas canvas, Line[] lines, SKFont font, SKPaint paint,
        float x, float y, float blockWidth, Alignment alignment)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.Value))
                continue;

            var lineX = alignment switch
            {
                Alignment.Center => x + (blockWidth - line.Width) * 0.5f,
                Alignment.Right => x + blockWidth - line.Width,
                _ => x,
            };

            canvas.DrawText(line.Value, lineX, y + line.Baseline, SKTextAlign.Left, font, paint);
        }
    }

    /// <summary>
    /// Create a RichTextKit TextBlock with proper font mapping for the given font.
    /// The TextBlock handles bidi reordering (UAX#9) and font fallback internally.
    /// </summary>
    public static TextBlock CreateTextBlock(string? text, SKFont font, Alignment alignment,
        SKColor textColor, float maxWidth = 0)
    {
        var textBlock = new TextBlock();
        if (font.Typeface != null)
            textBlock.FontMapper = new MapsuiFontMapper(font.Typeface);

        textBlock.AddText(text ?? "", new Style
        {
            FontFamily = font.Typeface?.FamilyName ?? "Arial",
            FontSize = font.Size,
            FontWeight = font.Typeface?.IsBold == true ? 700 : 400,
            FontItalic = font.Typeface?.IsItalic ?? false,
            TextColor = textColor,
        });

        textBlock.Alignment = alignment switch
        {
            Alignment.Center => TextAlignment.Center,
            Alignment.Right => TextAlignment.Right,
            _ => TextAlignment.Left,
        };

        if (maxWidth > 0)
            textBlock.MaxWidth = maxWidth;
        textBlock.Layout();

        return textBlock;
    }

    /// <summary>
    /// Paint a RichTextKit TextBlock onto the canvas. Handles bidi reordering,
    /// font fallback, and proper glyph shaping via HarfBuzz.
    /// </summary>
    public static void PaintTextBlock(SKCanvas canvas, TextBlock textBlock, float x, float y) =>
        textBlock.Paint(canvas, new SKPoint(x, y), new TextPaintOptions { Edging = SKFontEdging.Antialias });
}
