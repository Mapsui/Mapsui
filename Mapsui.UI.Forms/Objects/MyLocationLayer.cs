using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.UI.Forms.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Mapsui.UI.Objects
{
    public class MyLocationLayer : Layer
    {
        Feature moving;
        Feature still;
        byte[] bitmapMoving;
        byte[] bitmapStill;
        int bitmapMovingId = -1;
        int bitmapStillId = -1;

        Position myLocation = new Position(0, 0);

        public Position MyLocation
        {
            get
            {
                return myLocation;
            }
        }

        public double Direction { get; set; } = 0.0;
        public double Speed { get; } = 0.0;
        public double Scale { get; set; } = 1.0;

        public MyLocationLayer()
        {
            Enabled = false;

            bitmapMoving = CreateImage("MyLocationMoving");

            if (bitmapMoving != null)
            {
                // Register bitmap
                bitmapMovingId = BitmapRegistry.Instance.Register(new MemoryStream(bitmapMoving));
            }

            bitmapStill = CreateImage("MyLocationStill");

            if (bitmapStill != null)
            {
                // Register bitmap
                bitmapStillId = BitmapRegistry.Instance.Register(new MemoryStream(bitmapStill));
            }

            moving = new Feature
            {
                Geometry = myLocation.ToMapsui(),
                ["Label"] = "MyLocation moving",
            };

            moving.Styles.Clear();
            moving.Styles.Add(new SymbolStyle
            {
                Enabled = false,
                BitmapId = bitmapMovingId,
                SymbolScale = Scale,
                SymbolRotation = Direction,
                SymbolOffset = new Offset(16, 16),
                Opacity = 1,
            });

            still = new Feature
            {
                Geometry = myLocation.ToMapsui(),
                ["Label"] = "MyLocation still",
            };

            still.Styles.Clear();
            still.Styles.Add(new SymbolStyle
            {
                Enabled = true,
                BitmapId = bitmapStillId,
                SymbolScale = Scale,
                SymbolRotation = Direction,
                SymbolOffset = new Offset(16, 16),
                Opacity = 1,
            });

            DataSource = new MemoryProvider(new List<Feature> { moving, still });
            Style = null;
        }

        public bool UpdateMyLocation(Position newLocation, double newDirection, double newSpeed)
        {
            var modified = false;

            if (!myLocation.Equals(newLocation))
            {
                myLocation = newLocation;
                moving.Geometry = myLocation.ToMapsui();
                modified = true;
            }

            if (!Direction.Equals(newDirection))
            {
                Direction = newDirection;
                ((SymbolStyle)moving.Styles.First()).SymbolRotation = Direction;
                modified = true;
            }

            if (newSpeed > 0 && !((SymbolStyle)moving.Styles.First()).Enabled)
            {
                ((SymbolStyle)moving.Styles.First()).Enabled = true;
                ((SymbolStyle)still.Styles.First()).Enabled = false;
                modified = true;
            }

            if (newSpeed == 0 && !((SymbolStyle)still.Styles.First()).Enabled)
            {
                ((SymbolStyle)moving.Styles.First()).Enabled = false;
                ((SymbolStyle)still.Styles.First()).Enabled = true;
                modified = true;
            }

            return modified;
        }

        private byte[] CreateImage(string resName)
        {
            // First we have to create a bitmap from Svg code
            // Create a new SVG object
            var svg = new SkiaSharp.Extended.Svg.SKSvg();
            var assembly = typeof(Pin).GetTypeInfo().Assembly;
            // Load the SVG document
            Stream stream = assembly.GetManifestResourceStream($"Mapsui.UI.Images.{resName}.svg");
            if (stream == null)
                return null;
            svg.Load(stream);
            // Create bitmap to hold canvas
            var info = new SKImageInfo((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height) { AlphaType = SKAlphaType.Premul };
            var bitmap = new SKBitmap(info);
            var canvas = new SKCanvas(bitmap);
            // Now draw Svg image to bitmap
            using (var paint = new SKPaint())
            {
                // Replace color while drawing
                //paint.ColorFilter = SKColorFilter.CreateBlendMode(Color.ToSKColor(), SKBlendMode.SrcIn); // use the source color
                canvas.Clear();
                canvas.DrawPicture(svg.Picture, paint);
            }
            // Now convert canvas to bitmap
            byte[] bitmapData;

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                bitmapData = data.ToArray();
            }
            
            return bitmapData;
        }
    }
}
