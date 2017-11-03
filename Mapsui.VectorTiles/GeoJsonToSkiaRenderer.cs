using System;
using System.Collections.Generic;
using System.IO;
using GeoJSON.Net;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Mapsui.Projection;
using SkiaSharp;

namespace Mapsui.VectorTiles
{
    class GeoJsonToSkiaRenderer : IGeoJsonRenderer
    {
        private readonly int _canvasWidth;
        private readonly int _canvasHeight;
        private readonly float _extentMinX;
        private readonly float _extentMinY;
        private readonly float _extentWidth;
        private readonly float _extentHeight;
        private readonly Random _random = new Random(4321);

        public GeoJsonToSkiaRenderer(int canvasWidth, int canvasHeight, double[] boundingBox)
        {
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
            _extentMinX = (float)boundingBox[0];
            _extentMinY = (float)boundingBox[1];
            _extentWidth = (float)boundingBox[2] - _extentMinX;
            _extentHeight = (float)boundingBox[3] - _extentMinY;
        }

        public byte[] Render(IEnumerable<FeatureCollection> featureCollections)
        {
            using (var surface = SKSurface.Create(
                _canvasWidth, _canvasHeight, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul))
            {
                foreach (var featureCollection in featureCollections)
                {
                    foreach (var feature in featureCollection.Features)
                    {
                        if (feature.Geometry.Type == GeoJSONObjectType.Polygon)
                        {
                            var polygon = (Polygon)feature.Geometry;

                            DrawPolygon(polygon, surface);
                        }
                        else if (feature.Geometry.Type == GeoJSONObjectType.MultiPolygon)
                        {
                            var multiPolygon = (MultiPolygon)feature.Geometry;

                            foreach (var polygon in multiPolygon.Coordinates)
                            {
                                DrawPolygon(polygon, surface);
                            }
                        }
                    }
                }
                return ToBytes(surface);
            }
        }

        private void DrawPolygon(Polygon polygon, SKSurface surface)
        {
            foreach (var lineString in polygon.Coordinates)
            {
                var matrix = CreateTransformMatrix(_canvasWidth, _canvasHeight, _extentMinX, _extentMinY, _extentWidth,
                    _extentHeight);

                surface.Canvas.SetMatrix(matrix);

                var randomColor = new SKColor(
                    (byte) (255 * _random.NextDouble()), 
                    (byte) (255 * _random.NextDouble()),
                    (byte) (255 * _random.NextDouble()));

                using (var brush = new SKPaint { Color = randomColor })
                {
                    surface.Canvas.DrawPath(ToSkia(lineString), brush);
                }
            }
        }

        private static SKPath ToSkia(LineString lineString)
        {
            var result = new SKPath();

            foreach (var coordinate in lineString.Coordinates)
            {
                var position = coordinate;
                var sphericalPoint = SphericalMercator.FromLonLat(position.Longitude, position.Latitude);

                if (result.Points.Length == 0)
                    result.MoveTo((float)sphericalPoint.X, (float)sphericalPoint.Y);
                else
                   result.LineTo((float)sphericalPoint.X, (float)sphericalPoint.Y);
            }

            return result;
        }

        private static SKMatrix CreateTransformMatrix(int canvasWidth, int canvasHeight, 
            float minX, float minY, float width, float height)
        {
            // The code below needs no comments, it is fully intuitive.
            // I wrote in in one go and it ran correctly right away.
            var matrix = SKMatrix.MakeIdentity();

             var flipMatrix = SKMatrix.MakeScale(1, -1);
            SKMatrix.PostConcat(ref matrix, flipMatrix);

            var maxY = minY + height; // because inverted y
            var translationMatrix = SKMatrix.MakeTranslation(-minX, maxY);
            SKMatrix.PostConcat(ref matrix, translationMatrix);

            var scaleMatrix = SKMatrix.MakeScale(canvasWidth / width, canvasHeight / height);
            SKMatrix.PostConcat(ref matrix, scaleMatrix);

            return matrix;
        }
        
        private static byte[] ToBytes(SKSurface surface)
        {
            using (var image = surface.Snapshot())
            {
                using (var data = image.Encode())
                {
                    var memoryStream = new MemoryStream();
                    data.SaveTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
