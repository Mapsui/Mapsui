using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.MapInfo;

public class CustomCalloutStyleSample : ISample
{
    public string Name => "CustomCalloutStyle";
    public string Category => "MapInfo";

    private const string _customStyleLayerName = "Custom Callout Layer";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
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
        var titleFontColor = Color.Black;
        var subtitle = "subtitle";
        var subtitleFontColor = Color.Gray;

        using var titleTypeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        using var subtitleTypeface = SKTypeface.FromFamilyName(null, SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

        using var titleSkFont = new SKFont { Typeface = titleTypeface, Size = 15 };
        using var subtitleSkFont = new SKFont { Typeface = subtitleTypeface, Size = 12 };

        using var titlePaint = new SKPaint { Color = titleFontColor.ToSkia(), IsAntialias = true };
        using var subtitlePaint = new SKPaint { Color = subtitleFontColor.ToSkia(), IsAntialias = true };

        // Measure text
        titleSkFont.MeasureText(title, out var titleBounds, titlePaint);
        subtitleSkFont.MeasureText(subtitle, out var subtitleBounds, subtitlePaint);

        var titleWidth = titleBounds.Right - titleBounds.Left;
        var subtitleWidth = subtitleBounds.Right - subtitleBounds.Left;
        var subtitleHeight = subtitleBounds.Bottom - subtitleBounds.Top;

        // Use tight glyph height plus a small explicit gap for a compact but readable separation.
        var titleLineHeight = (titleBounds.Bottom - titleBounds.Top) + titleSkFont.Size * 0.25f;

        var width = Math.Max(titleWidth, subtitleWidth);
        var height = titleLineHeight + subtitleHeight;

        // Draw text to canvas
        using var recorder = new SKPictureRecorder();
        using var canvas = recorder.BeginRecording(new SKRect(0, 0, width, height));

        canvas.DrawText(title, -titleBounds.Left, -titleBounds.Top, SKTextAlign.Left, titleSkFont, titlePaint);
        canvas.DrawText(subtitle, -subtitleBounds.Left, titleLineHeight - subtitleBounds.Top, SKTextAlign.Left, subtitleSkFont, subtitlePaint);

        return recorder.EndRecording();
    }
}
