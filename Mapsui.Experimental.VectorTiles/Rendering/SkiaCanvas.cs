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

namespace Mapsui.Experimental.VectorTiles.Rendering;

public sealed class SkiaCanvas : ICanvas, IDisposable
{
    private int _width;

    private int _height;

    private SKSurface _surface;

    private Rect _clipRectangle;

    private List<IntPoint> _clipRectanglePath;

    private readonly ConcurrentDictionary<string, SKTypeface> _fontPairs = new();

    private static readonly object SFontLock = new object();

    private readonly List<Rect> _textRectangles = new List<Rect>();

    public bool ClipOverflow { get; set; }

    public SKColor BackgroundColor { get; private set; } = SKColors.White;

    public SkiaCanvas(int width, int height)
    {
        _width = width;
        _height = height;
        _surface = SKSurface.Create(new SKImageInfo(width, _height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
    }

    public void StartDrawing(double width, double height)
    {
        // Ignore width and height. It is in the interface but initialization was move to the constructor.

        double num = -5.0;
        _clipRectangle = new Rect(num, num, _width - num * 2.0, _height - num * 2.0);
        int num2 = 4;
        List<IntPoint> list = new List<IntPoint>(num2);
        CollectionsMarshal.SetCount(list, num2);
        Span<IntPoint> span = CollectionsMarshal.AsSpan(list);
        int num3 = 0;
        span[num3] = new IntPoint((int)_clipRectangle.Top, (int)_clipRectangle.Left);
        num3++;
        span[num3] = new IntPoint((int)_clipRectangle.Top, (int)_clipRectangle.Right);
        num3++;
        span[num3] = new IntPoint((int)_clipRectangle.Bottom, (int)_clipRectangle.Right);
        num3++;
        span[num3] = new IntPoint((int)_clipRectangle.Bottom, (int)_clipRectangle.Left);
        _clipRectanglePath = list;
    }

    public void DrawBackground(SKColor color)
    {
        BackgroundColor = color;
        _surface.Canvas.Clear(SKColorFactory.MakeColor(color.Red, color.Green, color.Blue, color.Alpha, "DrawBackground"));
    }

    private SKStrokeCap ConvertCap(PenLineCap cap)
    {
        return cap switch
        {
            PenLineCap.Flat => SKStrokeCap.Butt,
            PenLineCap.Round => SKStrokeCap.Round,
            _ => SKStrokeCap.Square,
        };
    }

    private double Clamp(double number, double min = 0.0, double max = 1.0)
    {
        return Math.Max(min, Math.Min(max, number));
    }

    private List<List<Point>> ClipPolygon(List<Point> geometry)
    {
        Clipper clipper = new Clipper();
        List<IntPoint> list = new List<IntPoint>();
        foreach (Point item in geometry)
        {
            list.Add(new IntPoint((int)item.X, (int)item.Y));
        }

        clipper.AddPolygon(list, PolyType.ptSubject);
        clipper.AddPolygon(_clipRectanglePath, PolyType.ptClip);
        List<List<IntPoint>> list2 = new List<List<IntPoint>>();
        if (clipper.Execute(ClipType.ctIntersection, list2, PolyFillType.pftNonZero, PolyFillType.pftEvenOdd) && list2.Count > 0)
        {
            return list2.Select((List<IntPoint> s) => s.Select((IntPoint item) => new Point(item.X, item.Y)).ToList()).ToList();
        }

        return null;
    }

    private SKPath GetPathFromGeometry(List<Point> geometry)
    {
        SKPath sKPath = new SKPath
        {
            FillType = SKPathFillType.EvenOdd
        };
        Point point = geometry[0];
        sKPath.MoveTo((float)point.X, (float)point.Y);
        foreach (Point item in geometry.Skip(1))
        {
            sKPath.LineTo((float)item.X, (float)item.Y);
        }

        return sKPath;
    }

    private static bool IsLeftToRight(List<Point> geometry)
    {
        Point point = geometry[0];
        Point point2 = geometry[geometry.Count - 1];
        return point.X <= point2.X;
    }

    private static bool IsTopToBottom(List<Point> geometry)
    {
        Point point = geometry[0];
        Point point2 = geometry[geometry.Count - 1];
        return point.Y > point2.Y;
    }

    public void DrawLineString(List<Point> geometry, Brush style)
    {
        if (ClipOverflow)
        {
            geometry = LineClipper.ClipPolyline(geometry, _clipRectangle);
            if (geometry == null)
            {
                return;
            }
        }

        using var pathFromGeometry = GetPathFromGeometry(geometry);
        if (pathFromGeometry == null)
        {
            return;
        }

        SKColor lineColor = style.Paint.LineColor;
        using var sKPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = ConvertCap(style.Paint.LineCap),
            StrokeWidth = (float)style.Paint.LineWidth,
            Color = SKColorFactory.MakeColor(lineColor.Red, lineColor.Green, lineColor.Blue, (byte)Clamp((double)(int)lineColor.Alpha * style.Paint.LineOpacity, 0.0, 255.0), "DrawLineString"),
            IsAntialias = true
        };
        if (style.Paint.LineDashArray.Any())
        {
            SKPathEffect pathEffect = SKPathEffect.CreateDash(style.Paint.LineDashArray.Select((double n) => (float)n).ToArray(), 0f);
            sKPaint.PathEffect = pathEffect;
        }

        _surface.Canvas.DrawPath(pathFromGeometry, sKPaint);
    }

    private SKTextAlign ConvertAlignment(TextAlignment alignment)
    {
        return alignment switch
        {
            TextAlignment.Center => SKTextAlign.Center,
            TextAlignment.Left => SKTextAlign.Left,
            TextAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Center,
        };
    }

    private SKPaint CreateTextStrokePaint(Brush style)
    {
        SKColor textStrokeColor = style.Paint.TextStrokeColor;
        return new SKPaint
        {
            IsStroke = true,
            StrokeWidth = (float)style.Paint.TextStrokeWidth,
            Color = SKColorFactory.MakeColor(textStrokeColor.Red, textStrokeColor.Green, textStrokeColor.Blue, (byte)Clamp((double)(int)textStrokeColor.Alpha * style.Paint.TextOpacity, 0.0, 255.0), "GetTextStrokePaint"),
            TextSize = (float)style.Paint.TextSize,
            IsAntialias = true,
            TextEncoding = SKTextEncoding.Utf32,
            TextAlign = ConvertAlignment(style.Paint.TextJustify),
            Typeface = GetFont(style.Paint.TextFont, style)
        };
    }

    private SKPaint GetTextPaint(Brush style)
    {
        SKColor textColor = style.Paint.TextColor;
        return new SKPaint
        {
            Color = SKColorFactory.MakeColor(textColor.Red, textColor.Green, textColor.Blue, (byte)Clamp((double)(int)textColor.Alpha * style.Paint.TextOpacity, 0.0, 255.0), "GetTextPaint"),
            TextSize = (float)style.Paint.TextSize,
            IsAntialias = true,
            TextEncoding = SKTextEncoding.Utf32,
            TextAlign = ConvertAlignment(style.Paint.TextJustify),
            Typeface = GetFont(style.Paint.TextFont, style),
            HintingLevel = SKPaintHinting.Normal
        };
    }

    private string TransformText(string text, Brush style)
    {
        if (text.Length == 0)
        {
            return string.Empty;
        }

        if (style.Paint.TextTransform == TextTransform.Uppercase)
        {
            text = text.ToUpper();
        }
        else if (style.Paint.TextTransform == TextTransform.Lowercase)
        {
            text = text.ToLower();
        }

        using var textPaint = GetTextPaint(style);
        text = BreakText(text, textPaint, style);
        return text;
    }

    private string BreakText(string input, SKPaint paint, Brush style)
    {
        string text = input;
        string text2 = string.Empty;
        do
        {
            long num = paint.BreakText(text, (float)(style.Paint.TextMaxWidth * style.Paint.TextSize));
            if (num == text.Length)
            {
                text2 += text.Trim();
                break;
            }

            int num2 = text.LastIndexOf(' ', (int)(num - 1));
            if ((uint)(num2 - -1) <= 1u)
            {
                text2 += text.Trim();
                break;
            }

            text2 = text2 + text.Substring(0, num2).Trim() + "\n";
            text = text.Substring(num2, text.Length - num2);
        }
        while (text.Length > 0);
        return text2.Trim();
    }

    private bool TextCollides(Rect rectangle)
    {
        foreach (Rect textRectangle in _textRectangles)
        {
            if (textRectangle.IntersectsWith(rectangle))
            {
                return true;
            }
        }

        return false;
    }

    private SKTypeface GetFont(string[] familyNames, Brush style)
    {
        lock (SFontLock)
        {
            foreach (string text in familyNames)
            {
                if (_fontPairs.TryGetValue(text, out var value))
                {
                    return value;
                }

                if (VectorStyleReader.TryGetFont(text, out var stream))
                {
                    SKTypeface sKTypeface = SKTypeface.FromStream(stream);
                    if (sKTypeface != null)
                    {
                        _fontPairs[text] = sKTypeface;
                        return sKTypeface;
                    }
                }

                SKTypeface sKTypeface2 = SKTypeface.FromFamilyName(text);
                if (sKTypeface2.FamilyName == text)
                {
                    _fontPairs[text] = sKTypeface2;
                    return sKTypeface2;
                }
            }

            SKTypeface sKTypeface3 = SKTypeface.FromFamilyName(familyNames.First());
            _fontPairs[familyNames.First()] = sKTypeface3;
            return sKTypeface3;
        }
    }

    private void QualifyTypeface(Brush style, SKPaint paint)
    {
        ushort[] array = new ushort[paint.Typeface.CountGlyphs(style.Text)];
        if (array.Length >= style.Text.Length)
        {
            return;
        }

        SKFontManager sKFontManager = SKFontManager.Default;
        int num;
        using var sKTypeface = sKFontManager.MatchCharacter(style.Text[array.Length]);
        if (sKTypeface != null)
        {
            paint.Typeface = sKTypeface;
            array = new ushort[sKTypeface.CountGlyphs(style.Text)];
            if (array.Length < style.Text.Length)
            {
                num = (array.Length != 0) ? array.Length : 0;
                style.Text = style.Text.Substring(0, num);
            }
        }
    }

    public void DrawText(Point geometry, Brush style)
    {
        _ = style.Paint.TextOptional;
        using var textPaint = GetTextPaint(style);
        QualifyTypeface(style, textPaint);
        using var textStrokePaint = CreateTextStrokePaint(style);
        string[] array = TransformText(style.Text, style).Split('\n');
        if (array.Length != 0)
        {
            string s = array.OrderBy((string line) => line.Length).Last();
            byte[] bytes = Encoding.UTF32.GetBytes(s);
            int num = (int)textPaint.MeasureText(bytes);
            int num2 = (int)(geometry.X - (double)(num / 2));
            int num3 = (int)(geometry.Y - style.Paint.TextSize / 2.0 * (double)array.Length);
            int num4 = (int)(style.Paint.TextSize * (double)array.Length);
            Rect rect = new Rect(num2, num3, num, num4);
            rect.Inflate(5.0, 5.0);
            if ((ClipOverflow && !_clipRectangle.Contains(rect)) || TextCollides(rect))
            {
                return;
            }

            _textRectangles.Add(rect);
        }

        int num5 = 0;
        string[] array2 = array;
        foreach (string text in array2)
        {
            float num7 = (float)((double)num5 * style.Paint.TextSize) - (float)array.Length * (float)style.Paint.TextSize / 2f + (float)style.Paint.TextSize;
            SKPoint p = new SKPoint((float)geometry.X + (float)(style.Paint.TextOffset.X * style.Paint.TextSize), (float)geometry.Y + (float)(style.Paint.TextOffset.Y * style.Paint.TextSize) + num7);
            if (style.Paint.TextStrokeWidth != 0.0)
            {
                _surface.Canvas.DrawText(text, p, textStrokePaint);
            }

            _surface.Canvas.DrawText(text, p, textPaint);
            num5++;
        }
    }

    private double GetPathLength(List<Point> path)
    {
        double num = 0.0;
        for (int i = 0; i < path.Count - 2; i++)
        {
            double length = Subtract(path[i], path[i + 1]).Length;
            num += length;
        }

        return num;
    }

    private Vector Subtract(Point point1, Point point2)
    {
        return new Vector(point1.X - point2.X, point1.Y - point2.Y);
    }

    private double GetAbsoluteDiff2Angles(double x, double y, double c = Math.PI)
    {
        return c - Math.Abs(Math.Abs(x - y) % 2.0 * c - c);
    }

    private bool CheckPathSqueezing(List<Point> path, double textHeight)
    {
        double y = 0.0;
        for (int i = 0; i < path.Count - 2; i++)
        {
            Vector vector = Subtract(path[i], path[i + 1]);
            double num = Math.Atan2(vector.Y, vector.X);
            if (Math.Abs(GetAbsoluteDiff2Angles(num, y)) > Math.PI / 3.0)
            {
                return true;
            }

            y = num;
        }

        return false;
    }

    private void DebugRectangle(Rect rectangle, SKColor color)
    {
    }

    public void DrawTextOnPath(List<Point> geometry, Brush style)
    {
        geometry = LineClipper.ClipPolyline(geometry, _clipRectangle);
        if (geometry == null)
        {
            return;
        }

        using var pathFromGeometry = CreatePathFromGeometry(geometry);

        string text = TransformText(style.Text, style);
        if (CheckPathSqueezing(geometry, style.Paint.TextSize))
        {
            return;
        }

        SKRect bounds = pathFromGeometry.Bounds;
        double num = 2.0;
        double num2 = (double)bounds.Left - style.Paint.TextSize - num;
        double num3 = (double)bounds.Top - style.Paint.TextSize - num;
        double num4 = (double)bounds.Right + style.Paint.TextSize + num;
        double num5 = (double)bounds.Bottom + style.Paint.TextSize + num;
        Rect rect = new Rect(num2, num3, num4 - num2, num5 - num3);
        if (TextCollides(rect))
        {
            DebugRectangle(rect, SKColorFactory.MakeColor(100, byte.MaxValue, 100, 128, "DrawTextOnPath"));
            return;
        }

        _textRectangles.Add(rect);
        if (style.Text.Length * style.Paint.TextSize * 0.2 >= GetPathLength(geometry))
        {
            DebugRectangle(rect, SKColorFactory.MakeColor(100, 100, byte.MaxValue, 128, "DrawTextOnPath"));
            return;
        }

        DebugRectangle(rect, SKColorFactory.MakeColor(byte.MaxValue, 0, 0, 150, "DrawTextOnPath"));
        SKPoint offset = new SKPoint((float)style.Paint.TextOffset.X, (float)style.Paint.TextOffset.Y);
        if (style.Paint.TextStrokeWidth != 0.0)
        {
            using var textStrokePaint = CreateTextStrokePaint(style);
            _surface.Canvas.DrawTextOnPath(text, pathFromGeometry, offset, warpGlyphs: true, textStrokePaint);
        }

        using var textPaint = GetTextPaint(style);
        _surface.Canvas.DrawTextOnPath(text, pathFromGeometry, offset, warpGlyphs: true, textPaint);
    }

    private SKPath CreatePathFromGeometry(List<Point> geometry)
    {
        if (IsLeftToRight(geometry))
        {
            return GetPathFromGeometry(geometry);
        }
        else
        {
            List<Point> list = new List<Point>(geometry);
            list.Reverse();
            return GetPathFromGeometry(list);
        }
    }

    public void DrawPoint(Point geometry, Brush style)
    {
        _ = style.Paint.IconImage;
    }

    public void DrawPolygon(List<Point> geometry, Brush style, SKColor? background = null)
    {
        List<List<Point>> list = (!ClipOverflow) ? new List<List<Point>> { geometry } : ClipPolygon(geometry);
        if (list == null)
        {
            return;
        }

        foreach (List<Point> item in list)
        {
            using var pathFromGeometry = GetPathFromGeometry(item);
            if (pathFromGeometry == null)
            {
                break;
            }

            SKColor color = ((!background.HasValue || !IsClockwise(geometry)) ? SKColorFactory.MakeColor(style.Paint.FillColor.Red, style.Paint.FillColor.Green, style.Paint.FillColor.Blue, (byte)Clamp((double)(int)style.Paint.FillColor.Alpha * style.Paint.FillOpacity, 0.0, 255.0), "DrawPolygon") : background.Value);
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                StrokeCap = ConvertCap(style.Paint.LineCap),
                Color = color,
                IsAntialias = true,
            };
            _surface.Canvas.DrawPath(pathFromGeometry, paint);
        }
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

    public void DrawImage(byte[] imageData, Brush style)
    {
        try
        {
            SKBitmap bitmap = SKBitmap.Decode(imageData);
            _surface.Canvas.DrawBitmap(bitmap, new SKPoint(0f, 0f));
        }
        catch (Exception)
        {
        }
    }

    public void DrawUnknown(List<List<Point>> geometry, Brush style)
    {
    }

    public void DrawDebugBox(TileInfo tileData, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Stroke
        };
        _surface.Canvas.DrawRect(new SKRect(0f, 0f, _width, _height), paint);
        using var textPaint = new SKPaint
        {
            FakeBoldText = true,
            TextSize = 14f,
            Color = color,
            Style = SKPaintStyle.Stroke
        };

        var text = $"({tileData.X}, {tileData.Y}, {(int)tileData.Zoom})";
        _surface.Canvas.DrawText(text, new SKPoint(20f, 20f), textPaint);
    }

    public byte[] ToPngByteArray(int quality = 80)
    {
        using SKImage sKImage = _surface.Snapshot();
        using SKData sKData = sKImage.Encode(SKEncodedImageFormat.Png, quality);
        using MemoryStream memoryStream = new MemoryStream();
        sKData.SaveTo(memoryStream);
        return memoryStream.ToArray();
    }

    public SKImage ToSKImage()
    {
        return _surface.Snapshot();
    }

    public void Dispose()
    {
        _surface?.Dispose();
    }
}
