using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;
using System;

namespace Mapsui.Experimental.Rendering.Skia.DrawableRenderers;

/// <summary>
/// Two-step renderer for RasterStyle. No cache interaction inside the renderer —
/// caching is managed externally by the orchestrator.
/// <list type="bullet">
///   <item><description><see cref="CreateDrawable"/>: Decodes raster bytes to SKImage/SKPicture
///         (expensive, runs on background thread).</description></item>
///   <item><description><see cref="DrawDrawable"/>: Blits a cached raster image to the canvas
///         (fast, render thread).</description></item>
/// </list>
/// </summary>
public class TwoStepRasterStyleRenderer : ITwoStepStyleRenderer
{
    /// <inheritdoc />
    public IDrawableCache CreateCache() => new TileDrawableCache();

    /// <inheritdoc />
    public IDrawable? CreateDrawable(Viewport viewport, ILayer layer, IFeature feature,
        IStyle style, RenderService renderService)
    {
        if (feature is not RasterFeature rasterFeature)
            return null;

        var raster = rasterFeature.Raster;
        if (raster is null)
            return null;

        if (style is not RasterStyle rasterStyle)
            return null;

        var extent = feature.Extent;
        if (extent is null)
            return null;

        try
        {
#pragma warning disable IDISP001 // Dispose created - ownership transferred to caller via drawable
            var data = DecodeRaster(raster);
#pragma warning restore IDISP001
            var opacity = (float)(layer.Opacity * style.Opacity);
            return new RasterStyleDrawable(data, extent, opacity, rasterStyle.Outline);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Error creating raster drawable: {ex.Message}", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public void DrawDrawable(object canvas, Viewport viewport, IDrawable drawable, ILayer layer)
    {
        if (canvas is not SKCanvas skCanvas || drawable is not RasterStyleDrawable rasterDrawable)
            return;

        var extent = rasterDrawable.Extent;

        if (viewport.IsRotated())
        {
            skCanvas.SetMatrix(CreateRotationMatrix(viewport, extent, skCanvas.TotalMatrix));
            var destination = new SKRect(0.0f, 0.0f, (float)extent.Width, (float)extent.Height);
            DrawRaster(skCanvas, rasterDrawable, destination);
        }
        else
        {
            var destination = WorldToScreen(viewport, extent);
            DrawRaster(skCanvas, rasterDrawable, RoundToPixel(destination));
        }
    }

    /// <summary>
    /// Decodes raster data into an SKImage or SKPicture. This is the expensive operation
    /// that is performed on the background thread.
    /// </summary>
    private static SKObject DecodeRaster(MRaster raster)
    {
        if (raster.Data.IsSkp())
        {
            return SKPicture.Deserialize(raster.Data);
        }

        using var skData = SKData.CreateCopy(raster.Data);
        return SKImage.FromEncodedData(skData);
    }

    private static void DrawRaster(SKCanvas canvas, RasterStyleDrawable drawable, SKRect destination)
    {
        if (drawable.Data is SKImage skImage)
            BitmapRenderer.Draw(canvas, skImage, destination, drawable.Opacity);
        else if (drawable.Data is SKPicture skPicture)
            PictureRenderer.Draw(canvas, skPicture, destination, drawable.Opacity);
        else
            throw new InvalidOperationException("Unknown raster data type");

        if (drawable.Outline is not null)
        {
            var halfStrokeWidth = (float)drawable.Outline.Width / 2;
            destination.Inflate(-halfStrokeWidth, -halfStrokeWidth);
            using var paint = new SKPaint
            {
                Color = drawable.Outline.Color.ToSkia(),
                StrokeWidth = (float)drawable.Outline.Width,
                IsStroke = true
            };
            canvas.DrawRect(destination, paint);
        }
    }

    private static SKMatrix CreateRotationMatrix(Viewport viewport, MRect rect, SKMatrix priorMatrix)
    {
        var userRotation = SKMatrix.CreateRotationDegrees((float)viewport.Rotation);
        var focalPointOffset = SKMatrix.CreateTranslation(
            (float)(rect.Left - viewport.CenterX),
            (float)(viewport.CenterY - rect.Top));
        var zoomScale = SKMatrix.CreateScale((float)(1.0 / viewport.Resolution), (float)(1.0 / viewport.Resolution));
        var centerInScreen = SKMatrix.CreateTranslation((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));

        var matrix = SKMatrix.Concat(zoomScale, focalPointOffset);
        matrix = SKMatrix.Concat(userRotation, matrix);
        matrix = SKMatrix.Concat(centerInScreen, matrix);
        matrix = SKMatrix.Concat(priorMatrix, matrix);

        return matrix;
    }

    private static SKRect WorldToScreen(Viewport viewport, MRect rect)
    {
        var first = viewport.WorldToScreen(rect.Min.X, rect.Min.Y);
        var second = viewport.WorldToScreen(rect.Max.X, rect.Max.Y);
        return new SKRect
        (
            (float)Math.Min(first.X, second.X),
            (float)Math.Min(first.Y, second.Y),
            (float)Math.Max(first.X, second.X),
            (float)Math.Max(first.Y, second.Y)
        );
    }

    private static SKRect RoundToPixel(SKRect boundingBox)
    {
        return new SKRect(
            (float)Math.Round(boundingBox.Left),
            (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
            (float)Math.Round(boundingBox.Right),
            (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
    }
}
