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
using System.Linq;
using Topten.RichTextKit;
using IStyle = Mapsui.Styles.IStyle;

namespace Mapsui.Samples.Common.Maps.Styles;

public class CustomCalloutStyleSample : IMapControlSample
{
    public string Name => "Custom Callout Style";
    public string Category => "MapInfo";

    private const string _customStyleLayerName = "Custom Callout Layer";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        MapRenderer.RegisterStyleRenderer(typeof(CustomCalloutStyle), new CustomCalloutStyleRenderer());

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var points = RandomPointsBuilder.GenerateRandomPoints(map.Extent, 25, 9898);
        map.Layers.Add(CreateCalloutLayer(CreateFeatures(points)));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == _customStyleLayerName));
        map.Tapped += MapTapped;

        return map;
    }

    private static void MapTapped(object? s, MapEventArgs e)
    {
        var feature = e.GetMapInfo(e.Map.Layers.Where(l => l.Name == _customStyleLayerName)).Feature;
        if (feature is not null)
        {
            if (feature["show-callout"]?.ToString() == "true")
                feature["show-callout"] = "false";
            else
                feature["show-callout"] = "true";
            e.Handled = true;
        }
    }

    private static MemoryLayer CreateCalloutLayer(IEnumerable<IFeature> features) => new()
    {
        Name = _customStyleLayerName,
        Features = features,
        Style = new StyleCollection
        {
            Styles = {
                CreatePinSymbol(),
                new CustomCalloutStyle()
            },
        },
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

    private static ImageStyle CreatePinSymbol() => new()
    {
        Image = new Image
        {
            Source = "embedded://Mapsui.Resources.Images.pin.svg",
            SvgFillColor = Color.FromString("#4193CF"),
            SvgStrokeColor = Color.DimGrey,
        },
        RelativeOffset = new RelativeOffset(0.0, 0.5), // The symbols point should be at the geolocation.        
    };
}

public class CustomCalloutStyle : BaseStyle { }

public class CustomCalloutStyleRenderer : ISkiaStyleRenderer
{
    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style,
        RenderService renderService, long iteration)
    {
        if (feature is not PointFeature pointFeature)
            throw new Exception($"Callout style only applies the {nameof(PointFeature)}");
        if (feature["show-callout"]?.ToString() != "true")
            return false;

        var balloonDefinition = new CalloutBalloonDefinition { RectRadius = 10, ShadowWidth = 4 };
        var symbolOffset = new Offset(0, 52 * 1f);
        var centroid = feature.Extent?.Centroid;
        if (centroid is null)
            return false;
        var (x, y) = viewport.WorldToScreenXY(centroid.X, centroid.Y);
        // Create the custom content.
        using var content = CreateCalloutContent();
        // Create bubble around content.
        using var callout = balloonDefinition.CreateCallout(content);

        // Save state of the canvas, so we could move and rotate the canvas
        canvas.Save();

        canvas.Translate((float)(x - symbolOffset.X), (float)(y - symbolOffset.Y));
        var balloonBounds = balloonDefinition.GetBalloonBounds(content.GetSize());
        canvas.Translate((float)-balloonBounds.TailTip.X, (float)-balloonBounds.TailTip.Y);

        using var paint = new SKPaint() { IsAntialias = true };
        canvas.DrawPicture(callout, paint);

        canvas.Restore();

        return true;
    }

    public static SKPicture CreateCalloutContent()
    {
        // This method creates the content of the callout. The code
        // below is based on the CalloutStyle.Type = CalloutType.Detail
        // renderer, but this could be anything. The content is totally
        // up to you when working with a custom callout style.

        var title = "title";
        var titleFont = new Font { FontFamily = null, Size = 15, Italic = false, Bold = true };
        var TitleFontColor = Color.Black;
        var subtitle = "subtitle";
        var subtitleFont = new Font { FontFamily = null, Size = 12, Italic = false, Bold = true };
        var subtitleFontColor = Color.Gray;
        var maxWidth = 120;

        var styleSubtitle = new Topten.RichTextKit.Style();
        var styleTitle = new Topten.RichTextKit.Style();
        var textBlockTitle = new TextBlock();
        var textBlockSubtitle = new TextBlock();

        styleSubtitle.FontFamily = subtitleFont.FontFamily;
        styleSubtitle.FontSize = (float)subtitleFont.Size;
        styleSubtitle.FontItalic = subtitleFont.Italic;
        styleSubtitle.FontWeight = subtitleFont.Bold ? 700 : 400;
        styleSubtitle.TextColor = subtitleFontColor.ToSkia();

        textBlockSubtitle.AddText(subtitle, styleSubtitle);

        styleTitle.FontFamily = titleFont.FontFamily;
        styleTitle.FontSize = (float)titleFont.Size;
        styleTitle.FontItalic = titleFont.Italic;
        styleTitle.FontWeight = titleFont.Bold ? 700 : 400;
        styleTitle.TextColor = TitleFontColor.ToSkia();

        textBlockTitle.AddText(title, styleTitle);

        textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = maxWidth;
        // Layout TextBlocks
        textBlockTitle.Layout();
        textBlockSubtitle.Layout();
        // Get sizes
        var width = Math.Max(textBlockTitle.MeasuredWidth, textBlockSubtitle.MeasuredWidth);
        var height = textBlockTitle.MeasuredHeight + textBlockSubtitle.MeasuredHeight;
        // Now we have the correct width, so make a new layout cycle for text alignment
        textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = width;
        textBlockTitle.Layout();
        textBlockSubtitle.Layout();
        // Create bitmap from TextBlock
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, width, height));
        // Draw text to canvas
        textBlockTitle.Paint(canvas, new TextPaintOptions() { Edging = SKFontEdging.Antialias });
        textBlockSubtitle.Paint(canvas, new SKPoint(0, textBlockTitle.MeasuredHeight), new TextPaintOptions() { Edging = SKFontEdging.Antialias });
        return recorder.EndRecording();
    }
}
