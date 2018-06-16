using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Mapsui.UI.Objects
{
    public class MyLocationLayer : Layer
    {
        MapView mapView;
        Feature feature;

        private static int bitmapMovingId = -1;
        private static int bitmapStillId = -1;

        private const string animationMyLocationName = "animationMyLocationPosition";
        private const string animationMyDirectionName = "animationMyDirectionPosition";
        private Position animationMyLocationStart;
        private Position animationMyLocationEnd;

        private bool isMoving = false;

        /// <summary>
        /// Should be moving arrow or round circle displayed
        /// </summary>
        public bool IsMoving
        {
            get
            {
                return isMoving;
            }
            set
            {
                if (isMoving != value)
                {
                    isMoving = value;
                    ((SymbolStyle)feature.Styles.First()).BitmapId = isMoving ? bitmapMovingId : bitmapStillId;
                }
            }
        }

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

        public MyLocationLayer(MapView view)
        {
            if (view == null)
                throw new ArgumentNullException("MapView shouldn't be null");
            
            mapView = view;

            Enabled = false;

            var assembly = typeof(MyLocationLayer).GetTypeInfo().Assembly;

            if (bitmapMovingId == -1)
            {
                var bitmapMoving = assembly.GetManifestResourceStream($"Mapsui.UI.Forms.Images.MyLocationMoving.svg");

                if (bitmapMoving != null)
                {
                    // Register bitmap
                    bitmapMovingId = BitmapRegistry.Instance.Register(bitmapMoving);
                }
            }

            if (bitmapStillId == -1)
            {
                var bitmapStill = assembly.GetManifestResourceStream($"Mapsui.UI.Forms.Images.MyLocationStill.svg");

                if (bitmapStill != null)
                {
                    // Register bitmap
                    bitmapStillId = BitmapRegistry.Instance.Register(bitmapStill);
                }
            }

            feature = new Feature
            {
                Geometry = myLocation.ToMapsui(),
                ["Label"] = "MyLocation moving",
            };

            feature.Styles.Clear();
            feature.Styles.Add(new SymbolStyle
            {
                Enabled = true,
                BitmapId = bitmapStillId,
                SymbolScale = Scale,
                SymbolRotation = Direction,
                SymbolOffset = new Offset(0, 0),
                Opacity = 1,
            });

            DataSource = new MemoryProvider(new List<Feature> { feature });
            //Style = null;
        }

        // Update my location
        public void UpdateMyLocation(Position newLocation)
        {
            if (!MyLocation.Equals(newLocation))
            {
                // We have a location update, so abort last animation
                if (mapView.AnimationIsRunning(animationMyLocationName))
                    mapView.AbortAnimation(animationMyLocationName);

                // Save values for new animation
                animationMyLocationStart = MyLocation;
                animationMyLocationEnd = newLocation;

                var animation = new Animation((v) =>
                {
                    var deltaLat = (animationMyLocationEnd.Latitude - animationMyLocationStart.Latitude) * v;
                    var deltaLon = (animationMyLocationEnd.Longitude - animationMyLocationStart.Longitude) * v;
                    var modified = InternalUpdateMyLocation(new Position(animationMyLocationStart.Latitude + deltaLat, animationMyLocationStart.Longitude + deltaLon));
                    // Update viewport
                    if (modified && mapView.MyLocationFollow && mapView.MyLocationEnabled)
                        mapView.Map.Viewport.Center = MyLocation.ToMapsui();
                    // Refresh map
                    if (mapView.MyLocationEnabled && modified)
                        mapView.Refresh();
                }, 0.0, 1.0);

                // At the end, update viewport
                animation.Commit(mapView, animationMyLocationName, 100, 3000, finished: (s, v) => mapView.Map.RefreshData(true));
            }
        }

        public void UpdateMyDirection(double newDirection, double newViewportRotation)
        {
            var newRotation = newDirection - newViewportRotation;
            var oldRotation = ((SymbolStyle)feature.Styles.First()).SymbolRotation;

            if (newRotation != oldRotation)
            {
                Direction = newDirection;

                // We have a direction update, so abort last animation
                if (mapView.AnimationIsRunning(animationMyDirectionName))
                    mapView.AbortAnimation(animationMyDirectionName);

                var animation = new Animation((v) =>
                {
                    if (v != ((SymbolStyle)feature.Styles.First()).SymbolRotation)
                    {
                        ((SymbolStyle)feature.Styles.First()).SymbolRotation = v;
                        mapView.Refresh();
                    }
                }, oldRotation, newRotation);

                animation.Commit(mapView, animationMyDirectionName, 50, 500);
            }
        }

        public void UpdateMySpeed(double newSpeed)
        {
            var modified = false;

            if (newSpeed > 0 && !IsMoving)
            {
                IsMoving = true;
                modified = true;
            }

            if (newSpeed <= 0 && IsMoving)
            {
                IsMoving = false;
                modified = true;
            }

            if (modified)
                mapView.Refresh();
        }

        private bool InternalUpdateMyLocation(Position newLocation)
        {
            var modified = false;

            if (!myLocation.Equals(newLocation))
            {
                myLocation = newLocation;
                feature.Geometry = myLocation.ToMapsui();
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
            Stream stream = assembly.GetManifestResourceStream($"Mapsui.UI.Forms.Images.{resName}.svg");
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
