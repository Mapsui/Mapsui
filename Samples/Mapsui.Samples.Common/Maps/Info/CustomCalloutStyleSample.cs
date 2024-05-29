#pragma warning disable IDISP001
#pragma warning disable IDISP004
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using Topten.RichTextKit;
using IStyle = Mapsui.Styles.IStyle;

namespace Mapsui.Samples.Common.Maps.Styles;

public class CustomCalloutStyleSample : IMapControlSample
{
    public string Name => "Custom Callout Style";
    public string Category => "1";//!!!"Info";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();

        if (mapControl.Renderer is MapRenderer && !mapControl.Renderer.StyleRenderers.ContainsKey(typeof(CustomCalloutStyle)))
            mapControl.Renderer.StyleRenderers.Add(typeof(CustomCalloutStyle), new CustomCalloutStyleRenderer());
    }

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var points = RandomPointsBuilder.GenerateRandomPoints(map.Extent, 25, 9898);
        map.Layers.Add(CreateCalloutLayer(CreateFeatures(points)));

        map.Widgets.Add(new MapInfoWidget(map));
        map.Info += MapOnInfo;

        return map;
    }

    private static void MapOnInfo(object? sender, MapInfoEventArgs e)
    {
        var feature = e.MapInfo?.Feature;
        if (feature is not null)
        {
            if (feature["show-callout"]?.ToString() == "true")
                feature["show-callout"] = "false";
            else
                feature["show-callout"] = "true";
        }
    }

    private static MemoryLayer CreateCalloutLayer(IEnumerable<IFeature> features) => new()
    {
        Name = "Custom Style Layer",
        Features = features,
        Style = new StyleCollection
        {
            Styles = {
                CreatePinSymbol(),
                new CustomCalloutStyle()
            },
        },
        IsMapInfoLayer = true
    };

    private static List<IFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
    {
        var features = new List<IFeature>();
        foreach (var point in randomPoints)
        {
            var feature = new PointFeature(point);
            features.Add(feature);
        }
        return features;
    }

    private static SymbolStyle CreatePinSymbol()
    {
        return new SymbolStyle
        {
            ImageSource = $"embedded://Mapsui.Resources.Images.pin.svg",
            SymbolOffset = new RelativeOffset(0.0, 0.5), // The symbols point should be at the geolocation.
            SvgFillColor = Color.FromString("#4193CF"),
            SvgStrokeColor = Color.DimGrey,
        };
    }
}

public class CustomCalloutStyle : BaseStyle { }

public class CustomCalloutStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        if (feature is not PointFeature pointFeature)
            throw new Exception($"Callout style only applies the {nameof(PointFeature)}");
        if (feature["show-callout"]?.ToString() != "true")
            return false;
        var calloutStyle = CreateCalloutStyle();
        var centroid = feature.Extent?.Centroid;
        if (centroid is null)
            return false;
        var (x, y) = viewport.WorldToScreenXY(centroid.X, centroid.Y);
        // Create content for callout
        var content = CreateCalloutContent(calloutStyle);
        // Create bubble around content
        var picture = CalloutStyleRenderer.CreateCallout(calloutStyle.ToCalloutOptions(), content);

        // Calc offset (relative or absolute)
        var symbolOffset = calloutStyle.SymbolOffset.CalcOffset(picture.CullRect.Width, picture.CullRect.Height);

        // Save state of the canvas, so we could move and rotate the canvas
        canvas.Save();

        // Move 0/0 to the Anchor point of Callout
        canvas.Translate((float)(x - symbolOffset.X), (float)(y - symbolOffset.Y));
        canvas.Scale((float)calloutStyle.SymbolScale, (float)calloutStyle.SymbolScale);

        // 0/0 are assumed at center of image, but Picture has 0/0 at left top position
        canvas.RotateDegrees(0);

        var bounds = CalloutStyleRenderer.GetBalloonBounds(calloutStyle.ToCalloutOptions(), content.GetSize());
        canvas.Translate((float)-bounds.TailTip.X, (float)-bounds.TailTip.Y);

        using var skPaint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(picture, skPaint);

        canvas.Restore();

        return true;
    }

    private static CalloutStyle CreateCalloutStyle() => new()
    {
        Title = "title",
        TitleFont = { FontFamily = null, Size = 15, Italic = false, Bold = true },
        TitleFontColor = Color.Black,
        Subtitle = "subtitle",
        SubtitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
        SubtitleFontColor = Color.Gray,
        Type = CalloutType.Detail,
        MaxWidth = 120,
        RectRadius = 10,
        ShadowWidth = 4,
        Enabled = false,
        SymbolOffset = new Offset(0, 52 * 1f)
    };

    /// <summary>
    /// Update content for single and detail
    /// </summary>
    public static SKPicture CreateCalloutContent(CalloutStyle calloutStyle)
    {
        var styleSubtitle = new Topten.RichTextKit.Style();
        var styleTitle = new Topten.RichTextKit.Style();
        var textBlockTitle = new TextBlock();
        var textBlockSubtitle = new TextBlock();

        styleSubtitle.FontFamily = calloutStyle.SubtitleFont.FontFamily;
        styleSubtitle.FontSize = (float)calloutStyle.SubtitleFont.Size;
        styleSubtitle.FontItalic = calloutStyle.SubtitleFont.Italic;
        styleSubtitle.FontWeight = calloutStyle.SubtitleFont.Bold ? 700 : 400;
        styleSubtitle.TextColor = calloutStyle.SubtitleFontColor.ToSkia();

        textBlockSubtitle.AddText(calloutStyle.Subtitle, styleSubtitle);
        textBlockSubtitle.Alignment = calloutStyle.SubtitleTextAlignment.ToRichTextKit();

        styleTitle.FontFamily = calloutStyle.TitleFont.FontFamily;
        styleTitle.FontSize = (float)calloutStyle.TitleFont.Size;
        styleTitle.FontItalic = calloutStyle.TitleFont.Italic;
        styleTitle.FontWeight = calloutStyle.TitleFont.Bold ? 700 : 400;
        styleTitle.TextColor = calloutStyle.TitleFontColor.ToSkia();

        textBlockTitle.Alignment = calloutStyle.TitleTextAlignment.ToRichTextKit();
        textBlockTitle.AddText(calloutStyle.Title, styleTitle);

        textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = (float)calloutStyle.MaxWidth;
        // Layout TextBlocks
        textBlockTitle.Layout();
        textBlockSubtitle.Layout();
        // Get sizes
        var width = Math.Max(textBlockTitle.MeasuredWidth, textBlockSubtitle.MeasuredWidth);
        var height = textBlockTitle.MeasuredHeight + textBlockSubtitle.MeasuredHeight + (float)calloutStyle.Spacing;
        // Now we have the correct width, so make a new layout cycle for text alignment
        textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = width;
        textBlockTitle.Layout();
        textBlockSubtitle.Layout();
        // Create bitmap from TextBlock
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, width, height));
        // Draw text to canvas
        textBlockTitle.Paint(canvas, new TextPaintOptions() { Edging = SKFontEdging.Antialias });
        textBlockSubtitle.Paint(canvas, new SKPoint(0, textBlockTitle.MeasuredHeight + (float)calloutStyle.Spacing), new TextPaintOptions() { Edging = SKFontEdging.Antialias });
        return recorder.EndRecording();
    }
}
