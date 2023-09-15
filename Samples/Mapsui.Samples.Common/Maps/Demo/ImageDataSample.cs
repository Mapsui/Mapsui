using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class ImageDataSample : ISample
{
    public string Name => "5 Image over Map Sample";
    public string Category => "Demo";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateImageLayer(map));
        map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Alignment.Center, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top });
        return map;
    }

    private static ILayer CreateImageLayer(Map map)
    {
        // Center is Hamburg, Germany
        var center = new MPoint(1113046, 7084790);

        map.Home = (n) => { n.CenterOnAndZoomTo(center, 150); };

        // We have to calc the angle difference to the equator (angle = 0), 
        // because EPSG:3857 is only there 1 m. At other angles, we
        // should calculate the correct length.
        (var _, var y) = ProjectionDefaults.Projection.Project(map.CRS ?? "EPSG:3857", "EPSG:4326", center.X, center.Y);

        // Data should be 100 x 100 km²
        var halfSizeOfSquare = 50000 * Math.Cos((90.0 - y) / 180.0 * Math.PI);

        // Choose which one you want
        // At the end data contains a PNG file with the image data
        var data = CreateSquareImageData(250, 250, (x, y) => GetColorForSquareDataPoint(x, y));
        // var data = CreateSquareImageDataFast(250, 250, (x, y) => GetColorForSquareDataPoint(x, y));
        // var data = CreatePolarImageData(500, 50, 6, (r, a) => GetColorForPolarDataPoint(r, a));
        
        // Place the PNG image on map
        var rect = new MRect(center.X - halfSizeOfSquare, center.Y - halfSizeOfSquare, center.X + halfSizeOfSquare, center.Y + halfSizeOfSquare);
        var raster = new MRaster(data, rect);
        var rasterFeature = new RasterFeature(raster);

        rasterFeature.Styles = new List<IStyle> { new RasterStyle() };

        var memoryLayer = new MemoryLayer("ImageData");
        var features = new List<RasterFeature> { rasterFeature };
        
        memoryLayer.Features = features;
        
        return memoryLayer;
    }

    private static byte[] CreateSquareImageData(int dataPointsInX, int dataPointsInY, Func<int, int, SKColor> getColorForDataPoint)
    {
        // Create a bitmap, that contains the image data
        using (SKBitmap bitmap = new SKBitmap(dataPointsInX, dataPointsInY))
        {
            for (int row = 0; row < dataPointsInY; row++)
                for (int col = 0; col < dataPointsInX; col++)
                {
                    bitmap.SetPixel(col, row, getColorForDataPoint(col, row));
                }

            using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }
    }

    // Uncomment this function, if you like to create larger bitmaps.
    // It is much faster than the SetPixel version, but needs unsafe
    // code option.
    /*
    private static byte[] CreateSquareImageDataFast(int dataPointsInX, int dataPointsInY, Func<int, int, SKColor> getColorForDataPoint)
    {
        // Create a bitmap, that contains the image data
        using (SKBitmap bitmap = new SKBitmap(dataPointsInX, dataPointsInY))
        {
            unsafe
            {
                var unsafePtr = (uint*)bitmap.GetPixels().ToPointer();

                for (int y = 0; y < dataPointsInY; y++)
                {
                    var offsetY = y * dataPointsInX;

                    for (int x = 0; x < dataPointsInX; x++)
                    {
                        unsafePtr[offsetY + x] = (uint)getColorForDataPoint(x, y);
                    }
                }
            }

            using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }
    }
    */

    private static byte[] CreatePolarImageData(int maxRadius, float stepRadius, float stepAngle, Func<float, float, SKColor> getColorForDataPoint)
    {
        // Create a bitmap, that contains the image data
        using (SKBitmap bitmap = new SKBitmap(maxRadius * 2, maxRadius * 2))
        {
            var center = new SKPoint(maxRadius, maxRadius);

            // Create a canvas, so it is possible to use normal SkiaSharp commands to draw
            using (var canvas = new SKCanvas(bitmap))
            {
                for (float angle = 0; angle < 360; angle += stepAngle)
                    for (float radius = 0; radius < maxRadius; radius += stepRadius)
                    {
                        var outerRadius = radius + stepRadius;

                        var innerRect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
                        var outerRect = new SKRect(center.X - outerRadius, center.Y - outerRadius, center.X + outerRadius, center.Y + outerRadius);

                        using (var path = new SKPath())
                            using (var paint = new SKPaint { IsStroke = false, StrokeWidth = 2 })
                            {
                                var startPoint = ConvertPolarToCartesian(radius, angle - stepAngle / 2) + center;

                                // Fill path
                                path.MoveTo(startPoint);
                                path.ArcTo(outerRect, angle - stepAngle / 2, stepAngle, false);
                                path.ArcTo(innerRect, angle + stepAngle / 2, -stepAngle, false);
                                path.Close();

                                paint.IsStroke = false;
                                paint.Color = getColorForDataPoint(radius, angle);

                                canvas.DrawPath(path, paint);

                                // Outline path
                                // Remove comments, if you want to have lines between the fields
                                //paint.IsStroke = true;
                                //paint.StrokeWidth = 2;
                                //paint.Color = SKColors.White;

                                //canvas.DrawPath(path, paint);
                            }
                    }

                using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return data.ToArray();
                }
            }
        }
    }

    private static SKPoint ConvertPolarToCartesian(double radius, double angle)
    {
        var x = radius * Math.Cos(angle * Math.PI / 180.0);
        var y = radius * Math.Sin(angle * Math.PI / 180.0);
        
        return new SKPoint((float)x, (float)y);
    }

    private static Random _random = new Random(1234);
    private static SKColor[] _dataColors = { SKColors.Blue, SKColors.LightBlue, SKColors.Green, SKColors.LightGreen, SKColors.Yellow, SKColors.Orange, SKColors.Red, SKColors.Pink };
    private static int _numOfDataColors = _dataColors.Length;

    private static SKColor GetColorForSquareDataPoint(int x, int y)
    {
        var value = _random.NextDouble();

        if (value < 0.3) 
        { 
            // Transparent
            return SKColors.White.WithAlpha(0);
        }

        var pos = _random.Next(_numOfDataColors);

        return _dataColors[pos].WithAlpha(128);
    }

    private static SKColor GetColorForPolarDataPoint(float r, float a)
    {
        var value = _random.NextDouble();

        if (value < 0.3)
        {
            // Transparent
            return SKColors.White.WithAlpha(0);
        }

        var pos = _random.Next(_numOfDataColors);

        return _dataColors[pos].WithAlpha(128);
    }
}
