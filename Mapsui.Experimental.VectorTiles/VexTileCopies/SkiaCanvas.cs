using Mapsui.Logging;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using VexTile.ClipperLib;
using VexTile.Renderer.Mvt.AliFlux;
using VexTile.Renderer.Mvt.AliFlux.Drawing;
using VexTile.Renderer.Mvt.AliFlux.Enums;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

/// <summary>
/// Canvas implementation for vector tile rendering.
/// Uses reusable paint objects to minimize allocations.
/// </summary>
public sealed class SkiaCanvas : ICanvas, IDisposable
{
    private readonly int _width;
    private readonly int _height;
    private readonly SKSurface _surface;
    private Rect _clipRectangle;
    private readonly List<IntPoint> _clipRectanglePath = new(4);
    private readonly List<Rect> _textRectangles = new();

    // Shared font cache across all canvas instances
    private static readonly ConcurrentDictionary<string, SKTypeface> _fontCache = new();

    // Reusable paint objects - configured per-use instead of recreating
    private readonly SKPaint _fillPaint = new() { IsAntialias = true };
    private readonly SKPaint _strokePaint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _textPaint = new() { IsAntialias = true };
    private readonly SKPaint _textStrokePaint = new() { IsAntialias = true, IsStroke = true };
    private readonly SKPaint _breakPaint = new();

    // Reusable path object
    private readonly SKPath _path = new() { FillType = SKPathFillType.EvenOdd };

    // Cache for dash array conversions - avoids repeated float[] allocation for shared style objects
    private IEnumerable<double>? _lastDashArray;
    private float[]? _lastDashFloats;
    private bool _lastDashIsNonEmpty;

    public SkiaCanvas(int width, int height)
    {
        _width = width;
        _height = height;
        _surface = SKSurface.Create(new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));

        // Initialize clip rectangle
        double margin = -5.0;
        _clipRectangle = new Rect(margin, margin, _width - margin * 2.0, _height - margin * 2.0);

        // Build clip path efficiently
        _clipRectanglePath.Clear();
        CollectionsMarshal.SetCount(_clipRectanglePath, 4);
        var span = CollectionsMarshal.AsSpan(_clipRectanglePath);
        span[0] = new IntPoint((int)_clipRectangle.Top, (int)_clipRectangle.Left);
        span[1] = new IntPoint((int)_clipRectangle.Top, (int)_clipRectangle.Right);
        span[2] = new IntPoint((int)_clipRectangle.Bottom, (int)_clipRectangle.Right);
        span[3] = new IntPoint((int)_clipRectangle.Bottom, (int)_clipRectangle.Left);
    }

    public bool ClipOverflow { get; set; }

    public SKColor BackgroundColor { get; private set; } = SKColors.White;

    public void DrawBackground(SKColor color)
    {
        BackgroundColor = color;
        _surface.Canvas.Clear(SKColorFactory.MakeColor(color.Red, color.Green, color.Blue, color.Alpha, "DrawBackground"));
    }

    public void DrawLineString(List<Point> geometry, Brush style)
    {
        if (ClipOverflow)
        {
            geometry = LineClipper.ClipPolyline(geometry, _clipRectangle);
            if (geometry == null)
                return;
        }

        if (geometry.Count == 0) return;

        SKColor lineColor = style.Paint.LineColor;
        var color = SKColorFactory.MakeColor(lineColor.Red, lineColor.Green, lineColor.Blue,
            (byte)Clamp(lineColor.Alpha * style.Paint.LineOpacity, 0.0, 255.0), "DrawLineString");

        _path.Reset();
        BuildPath(_path, geometry);

        _strokePaint.Reset();
        _strokePaint.IsAntialias = true;
        _strokePaint.Style = SKPaintStyle.Stroke;
        _strokePaint.StrokeCap = ConvertCap(style.Paint.LineCap);
        _strokePaint.StrokeWidth = (float)style.Paint.LineWidth;
        _strokePaint.Color = color;

        var dashArray = style.Paint.LineDashArray;
        if (!ReferenceEquals(_lastDashArray, dashArray))
        {
            _lastDashFloats = dashArray.Select(n => (float)n).ToArray();
            _lastDashIsNonEmpty = _lastDashFloats.Length > 0;
            _lastDashArray = dashArray;
        }
        if (_lastDashIsNonEmpty)
        {
            _strokePaint.PathEffect = SKPathEffect.CreateDash(_lastDashFloats!, 0f);
        }
        else
        {
            _strokePaint.PathEffect = null;
        }

        _surface.Canvas.DrawPath(_path, _strokePaint);
    }

    public void DrawPolygon(List<Point> geometry, Brush style, SKColor? background = null)
    {
        if (geometry.Count == 0)
            return;

        SKColor color = (!background.HasValue || !IsClockwise(geometry))
            ? SKColorFactory.MakeColor(style.Paint.FillColor.Red, style.Paint.FillColor.Green, style.Paint.FillColor.Blue,
                (byte)Clamp(style.Paint.FillColor.Alpha * style.Paint.FillOpacity, 0.0, 255.0), "DrawPolygon")
            : background.Value;

        _fillPaint.Reset();
        _fillPaint.IsAntialias = true;
        _fillPaint.Style = SKPaintStyle.Fill;
        _fillPaint.StrokeCap = ConvertCap(style.Paint.LineCap);
        _fillPaint.Color = color;

        _path.Reset();
        BuildPath(_path, geometry);

        _surface.Canvas.DrawPath(_path, _fillPaint);
    }

    public void DrawPoint(Point geometry, Brush style)
    {
        _ = style.Paint.IconImage;
    }

    public void DrawText(Point geometry, Brush style)
    {
        _ = style.Paint.TextOptional;
        var typeface = GetFont(style.Paint.TextFont, style);
        typeface = QualifyTypeface(style, typeface);
        using var font = new SKFont(typeface, (float)style.Paint.TextSize) { Hinting = SKFontHinting.Normal };

        ConfigureTextPaint(_textPaint, style);
        ConfigureTextStrokePaint(_textStrokePaint, style);

        var textAlign = ConvertAlignment(style.Paint.TextJustify);
        string[] array = TransformText(style.Text, style).Split('\n');
        if (array.Length != 0)
        {
            string s = array[0];
            for (var li = 1; li < array.Length; li++)
                if (array[li].Length > s.Length) s = array[li];
            int num = (int)font.MeasureText(s, _textPaint);
            int num2 = (int)(geometry.X - num / 2);
            int num3 = (int)(geometry.Y - style.Paint.TextSize / 2.0 * array.Length);
            int num4 = (int)(style.Paint.TextSize * array.Length);
            Rect rect = new Rect(num2, num3, num, num4);
            rect = rect.Inflate(5.0, 5.0);
            if ((ClipOverflow && !_clipRectangle.Contains(rect)) || TextCollides(rect))
                return;

            _textRectangles.Add(rect);
        }

        int num5 = 0;
        foreach (string text in array)
        {
            float num7 = (float)(num5 * style.Paint.TextSize) - array.Length * (float)style.Paint.TextSize / 2f + (float)style.Paint.TextSize;
            SKPoint p = new SKPoint((float)geometry.X + (float)(style.Paint.TextOffset.X * style.Paint.TextSize), (float)geometry.Y + (float)(style.Paint.TextOffset.Y * style.Paint.TextSize) + num7);
            if (style.Paint.TextStrokeWidth != 0.0)
            {
                _surface.Canvas.DrawText(text, p, textAlign, font, _textStrokePaint);
            }

            _surface.Canvas.DrawText(text, p, textAlign, font, _textPaint);
            num5++;
        }
    }

    public void DrawTextOnPath(List<Point> geometry, Brush style)
    {
        geometry = LineClipper.ClipPolyline(geometry, _clipRectangle);
        if (geometry == null)
            return;

        string text = TransformText(style.Text, style);
        if (CheckPathSqueezing(geometry))
            return;

        using var pathFromGeometry = CreatePathFromGeometry(geometry);
        SKRect bounds = pathFromGeometry.Bounds;
        double num = 2.0;
        double num2 = (double)bounds.Left - style.Paint.TextSize - num;
        double num3 = (double)bounds.Top - style.Paint.TextSize - num;
        double num4 = (double)bounds.Right + style.Paint.TextSize + num;
        double num5 = (double)bounds.Bottom + style.Paint.TextSize + num;
        Rect rect = new Rect(num2, num3, num4 - num2, num5 - num3);
        if (TextCollides(rect))
            return;

        _textRectangles.Add(rect);

        if (style.Text.Length * style.Paint.TextSize * 0.2 >= GetPathLength(geometry))
            return;

        SKPoint offset = new SKPoint((float)style.Paint.TextOffset.X, (float)style.Paint.TextOffset.Y);
        using var font = CreateTextFont(style);
        var textAlign = ConvertAlignment(style.Paint.TextJustify);

        if (style.Paint.TextStrokeWidth != 0.0)
        {
            ConfigureTextStrokePaint(_textStrokePaint, style);
            _surface.Canvas.DrawTextOnPath(text, pathFromGeometry, offset, warpGlyphs: true, textAlign, font, _textStrokePaint);
        }

        ConfigureTextPaint(_textPaint, style);
        _surface.Canvas.DrawTextOnPath(text, pathFromGeometry, offset, warpGlyphs: true, textAlign, font, _textPaint);
    }

    public void DrawImage(byte[] imageData, Brush style)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(imageData);
            _surface.Canvas.DrawBitmap(bitmap, new SKPoint(0f, 0f));
        }
        catch (Exception)
        {
            Logger.Log(LogLevel.Error, "Failed to decode image data for DrawImage.");
        }
    }

    public void DrawUnknown(List<List<Point>> geometry, Brush style)
    {
    }

    public byte[] ToPngByteArray(int quality = 80)
    {
        using SKImage image = _surface.Snapshot();
        using SKData data = image.Encode(SKEncodedImageFormat.Png, quality);
        using MemoryStream stream = new MemoryStream();
        data.SaveTo(stream);
        return stream.ToArray();
    }

    public SKImage ToSKImage()
    {
        return _surface.Snapshot();
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _fillPaint?.Dispose();
        _strokePaint?.Dispose();
        _textPaint?.Dispose();
        _textStrokePaint?.Dispose();
        _path?.Dispose();
    }

    #region Private helper methods

    private static SKStrokeCap ConvertCap(PenLineCap cap) => cap switch
    {
        PenLineCap.Flat => SKStrokeCap.Butt,
        PenLineCap.Round => SKStrokeCap.Round,
        _ => SKStrokeCap.Square,
    };

    private static double Clamp(double number, double min = 0.0, double max = 1.0)
    {
        return Math.Max(min, Math.Min(max, number));
    }

    private static void BuildPath(SKPath path, List<Point> geometry, bool reverse = false)
    {
        if (geometry.Count == 0) return;
        if (!reverse)
        {
            path.MoveTo((float)geometry[0].X, (float)geometry[0].Y);
            for (var i = 1; i < geometry.Count; i++)
                path.LineTo((float)geometry[i].X, (float)geometry[i].Y);
        }
        else
        {
            var last = geometry.Count - 1;
            path.MoveTo((float)geometry[last].X, (float)geometry[last].Y);
            for (var i = last - 1; i >= 0; i--)
                path.LineTo((float)geometry[i].X, (float)geometry[i].Y);
        }
    }

    private static bool IsLeftToRight(List<Point> geometry)
    {
        Point point = geometry[0];
        Point point2 = geometry[geometry.Count - 1];
        return point.X <= point2.X;
    }

    private static bool IsClockwise(List<Point> polygon)
    {
        double num = 0.0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Point point = polygon[i];
            Point point2 = polygon[(i + 1) % polygon.Count];
            num += (point2.X - point.X) * (point2.Y + point.Y);
        }
        return num > 0.0;
    }

    private static SKTextAlign ConvertAlignment(TextAlignment alignment) => alignment switch
    {
        TextAlignment.Center => SKTextAlign.Center,
        TextAlignment.Left => SKTextAlign.Left,
        TextAlignment.Right => SKTextAlign.Right,
        _ => SKTextAlign.Center,
    };

    private static void ConfigureTextPaint(SKPaint paint, Brush style)
    {
        SKColor textColor = style.Paint.TextColor;
        paint.Reset();
        paint.Color = SKColorFactory.MakeColor(textColor.Red, textColor.Green, textColor.Blue,
            (byte)Clamp(textColor.Alpha * style.Paint.TextOpacity, 0.0, 255.0), "GetTextPaint");
        paint.IsAntialias = true;
    }

    private static void ConfigureTextStrokePaint(SKPaint paint, Brush style)
    {
        SKColor textStrokeColor = style.Paint.TextStrokeColor;
        paint.Reset();
        paint.IsStroke = true;
        paint.StrokeWidth = (float)style.Paint.TextStrokeWidth;
        paint.Color = SKColorFactory.MakeColor(textStrokeColor.Red, textStrokeColor.Green, textStrokeColor.Blue,
            (byte)Clamp(textStrokeColor.Alpha * style.Paint.TextOpacity, 0.0, 255.0), "GetTextStrokePaint");
        paint.IsAntialias = true;
    }

    private static SKFont CreateTextFont(Brush style)
    {
        var typeface = GetFont(style.Paint.TextFont, style);
        return new SKFont(typeface, (float)style.Paint.TextSize)
        {
            Hinting = SKFontHinting.Normal
        };
    }

    private string TransformText(string text, Brush style)
    {
        if (text.Length == 0)
            return string.Empty;

        if (style.Paint.TextTransform == TextTransform.Uppercase)
            text = text.ToUpper();
        else if (style.Paint.TextTransform == TextTransform.Lowercase)
            text = text.ToLower();

        using var font = CreateTextFont(style);
        text = BreakText(text, font, style, _breakPaint);
        return text;
    }

    private static string BreakText(string input, SKFont font, Brush style, SKPaint paint)
    {
        string text = input;
        var sb = new StringBuilder();
        do
        {
            long num = font.BreakText(text, (float)(style.Paint.TextMaxWidth * style.Paint.TextSize), paint);
            if (num == text.Length)
            {
                sb.Append(text.Trim());
                break;
            }

            int num2 = text.LastIndexOf(' ', (int)(num - 1));
            if ((uint)(num2 - -1) <= 1u)
            {
                sb.Append(text.Trim());
                break;
            }

            sb.Append(text.Substring(0, num2).Trim());
            sb.Append('\n');
            text = text.Substring(num2, text.Length - num2);
        }
        while (text.Length > 0);
        return sb.ToString().Trim();
    }

    private bool TextCollides(Rect rectangle)
    {
        foreach (Rect textRectangle in _textRectangles)
        {
            if (textRectangle.IntersectsWith(rectangle))
                return true;
        }
        return false;
    }

    private static SKTypeface GetFont(string[] familyNames, Brush style)
    {
        foreach (string text in familyNames)
        {
            if (_fontCache.TryGetValue(text, out var value))
                return value;

            if (VectorStyleReader.TryGetFont(text, out var stream))
            {
                SKTypeface sKTypeface = SKTypeface.FromStream(stream);
                if (sKTypeface != null)
                {
                    _fontCache[text] = sKTypeface;
                    return sKTypeface;
                }
            }

            SKTypeface sKTypeface2 = SKTypeface.FromFamilyName(text);
            if (sKTypeface2.FamilyName == text)
            {
                _fontCache[text] = sKTypeface2;
                return sKTypeface2;
            }
        }

        SKTypeface sKTypeface3 = SKTypeface.FromFamilyName(familyNames[0]);
        _fontCache[familyNames[0]] = sKTypeface3;
        return sKTypeface3;
    }

    private static SKTypeface QualifyTypeface(Brush style, SKTypeface typeface)
    {
        // CountGlyphs returns an int directly — no array allocation needed.
        int glyphCount = typeface.CountGlyphs(style.Text);
        if (glyphCount >= style.Text.Length)
            return typeface;

        SKFontManager sKFontManager = SKFontManager.Default;
        using var fallbackTypeface = sKFontManager.MatchCharacter(style.Text[glyphCount]);
        int fallbackGlyphCount = fallbackTypeface.CountGlyphs(style.Text);
        if (fallbackGlyphCount < style.Text.Length)
            style.Text = style.Text[..fallbackGlyphCount];
        return typeface;
    }

    private static double GetPathLength(List<Point> path)
    {
        double num = 0.0;
        for (int i = 0; i < path.Count - 2; i++)
        {
            double length = Subtract(path[i], path[i + 1]).Length;
            num += length;
        }
        return num;
    }

    private static Vector Subtract(Point point1, Point point2)
    {
        return new Vector(point1.X - point2.X, point1.Y - point2.Y);
    }

    private static double GetAbsoluteDiff2Angles(double x, double y, double c = Math.PI)
    {
        return c - Math.Abs(Math.Abs(x - y) % 2.0 * c - c);
    }

    private static bool CheckPathSqueezing(List<Point> path)
    {
        double y = 0.0;
        for (int i = 0; i < path.Count - 2; i++)
        {
            Vector vector = Subtract(path[i], path[i + 1]);
            double num = Math.Atan2(vector.Y, vector.X);
            if (Math.Abs(GetAbsoluteDiff2Angles(num, y)) > Math.PI / 3.0)
                return true;
            y = num;
        }
        return false;
    }

    private static SKPath CreatePathFromGeometry(List<Point> geometry)
    {
        var reverse = !IsLeftToRight(geometry);
        var path = new SKPath { FillType = SKPathFillType.EvenOdd };
        BuildPath(path, geometry, reverse);
        return path;
    }

    #endregion
}
