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

public class WeatherDataSample : ISample
{
    public string Name => "5 Weather Data Sample";
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
        map.Layers.Add(CreateWeatherDataLayer(map));
        map.Widgets.Add(new ScaleBarWidget(map) { TextAlignment = Alignment.Center, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top });
        return map;
    }

    private static ILayer CreateWeatherDataLayer(Map map)
    {
        // Center is Hamburg, Germany
        var center = new MPoint(1113046, 7084790);

        map.Home = (n) => { n.CenterOnAndZoomTo(center, 150); };

        // We have to calc the angle difference to the equator (angle = 0), 
        // because EPSG:3857 is only there 1 m. At other angles, we
        // should calculate the correct length.
        (var _, var y) = ProjectionDefaults.Projection.Project(map.CRS ?? "EPSG:3857", "EPSG:4326", center.X, center.Y);

        // Weather data should be 100 x 100 km²
        var halfSizeOfSquare = 50000 * Math.Cos((90.0 - y) / 180.0 * Math.PI);

        // Choose which one you want
        // At the end data contains a PNG file with the weather data
        //var data = CreateSquareWeatherData();
        var data = CreatePolarWeatherData();
        
        // Place the PNG image on map
        var rect = new MRect(center.X - halfSizeOfSquare, center.Y - halfSizeOfSquare, center.X + halfSizeOfSquare, center.Y + halfSizeOfSquare);
        var raster = new MRaster(data, rect);
        var rasterFeature = new RasterFeature(raster);

        rasterFeature.Styles = new List<IStyle> { new RasterStyle() };

        var memoryLayer = new MemoryLayer("WeatherData");
        var features = new List<RasterFeature> { rasterFeature };
        
        memoryLayer.Features = features;
        
        return memoryLayer;
    }

    private static byte[] CreateSquareWeatherData()
    {
        // Create a bitmap, that contains the weather data
        using (SKBitmap bitmap = new SKBitmap(100, 100))
        {
            for (int row = 0; row < 100; row++)
                for (int col = 0; col < 100; col++)
                {
                    bitmap.SetPixel(col, row, new SKColor((byte)(col << 1), 0, (byte)(row << 1), (byte)128));
                }

            using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }
    }

    private static byte[] CreatePolarWeatherData()
    {
        int maxRadius = 500;
        float stepRadius = 50.0f;
        float stepAngle = 6.0f;

        // Create a bitmap, that contains the weather data
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
                            using (var paint = new SKPaint { Color = new SKColor((byte)angle, 0, (byte)radius, 128), IsStroke = false, StrokeWidth = 2 })
                            {
                                var startPoint = CalcPoint(radius, angle - stepAngle / 2) + center;

                                // Fill path
                                path.MoveTo(startPoint);
                                path.ArcTo(outerRect, angle - stepAngle / 2, stepAngle, false);
                                path.ArcTo(innerRect, angle + stepAngle / 2, -stepAngle, false);
                                path.Close();

                                canvas.DrawPath(path, paint);

                                // Outline path
                                paint.IsStroke = true;
                                paint.Color = SKColors.White;

                                canvas.DrawPath(path, paint);
                            }
                    }

                using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return data.ToArray();
                }
            }
        }
    }

    private static SKPoint CalcPoint(double radius, double angle)
    {
        var x = radius * Math.Cos(angle * Math.PI / 180.0);
        var y = radius * Math.Sin(angle * Math.PI / 180.0);
        
        return new SKPoint((float)x, (float)y);
    }
}
