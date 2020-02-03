using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Mapsui.UI.Objects
{
    /// <summary>
    /// A layer to display a symbol for own location
    /// </summary>
    /// <remarks>
    /// There are two different symbols for own loaction: one is used when there isn't a change in position (still),
    /// and one is used, if the position changes (moving).
    /// </remarks>
    public class MyLocationLayer : MemoryLayer
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

        /// <summary>
        /// Position of location, that is displayed
        /// </summary>
        /// <value>Position of location</value>
        public Position MyLocation
        {
            get
            {
                return myLocation;
            }
        }

        /// <summary>
        /// Direction of device at location
        /// </summary>
        /// <value>Direction at location</value>
        public double Direction { get; set; } = 0.0;

        /// <summary>
        /// Speed of moving
        /// </summary>
        /// <value>Speed of moving</value>
        public double Speed { get; } = 0.0;

        /// <summary>
        /// Scale of symbol
        /// </summary>
        /// <value>Scale of symbol</value>
        public double Scale { get; set; } = 1.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Objects.MyLocationLayer"/> class
        /// with a starting location.
        /// </summary>
        /// <param name="view">MapView, to which this layer belongs</param>
        /// <param name="position">Position, where to start</param>
        public MyLocationLayer(MapView view, Position position) : this(view)
        {
            myLocation = position;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Objects.MyLocationLayer"/> class.
        /// </summary>
        /// <param name="view">MapView, to which this layer belongs</param>
        public MyLocationLayer(MapView view)
        {
            if (view == null)
                throw new ArgumentNullException("MapView shouldn't be null");
            
            mapView = view;

            Enabled = false;

            if (bitmapMovingId == -1)
            {
                var bitmapMoving = Mapsui.Utilities.EmbeddedResourceLoader.Load($"Images.MyLocationMoving.svg", typeof(MyLocationLayer));

                if (bitmapMoving != null)
                {
                    // Register bitmap
                    bitmapMovingId = BitmapRegistry.Instance.Register(bitmapMoving);
                }
            }

            if (bitmapStillId == -1)
            {
                var bitmapStill = Mapsui.Utilities.EmbeddedResourceLoader.Load($"Images.MyLocationStill.svg", typeof(MyLocationLayer));

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
            Style = null;
        }

        /// <summary>
        /// Updates my location
        /// </summary>
        /// <param name="newLocation">New location</param>
        public void UpdateMyLocation(Position newLocation, bool animated = true)
        {
            if (!MyLocation.Equals(newLocation))
            {
                // We have a location update, so abort last animation
                if (mapView.AnimationIsRunning(animationMyLocationName))
                    mapView.AbortAnimation(animationMyLocationName);

                if (animated)
                {
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
                            mapView._mapControl.Navigator.CenterOn(MyLocation.ToMapsui());
                    // Refresh map
                    if (mapView.MyLocationEnabled && modified)
                            mapView.Refresh();
                    }, 0.0, 1.0);

                    // At the end, update viewport
                    animation.Commit(mapView, animationMyLocationName, 100, 3000, finished: (s, v) => mapView.Map.RefreshData(mapView._mapControl.Viewport.Extent, mapView._mapControl.Viewport.Resolution, true));
                }
                else
                {
                    var modified = InternalUpdateMyLocation(newLocation);
                    // Update viewport
                    if (modified && mapView.MyLocationFollow && mapView.MyLocationEnabled)
                        mapView._mapControl.Navigator.CenterOn(MyLocation.ToMapsui());
                    // Refresh map
                    if (mapView.MyLocationEnabled && modified)
                        mapView.Refresh();
                }
            }
        }

        /// <summary>
        /// Updates my direction
        /// </summary>
        /// <param name="newDirection">New direction</param>
        /// <param name="newViewportRotation">New viewport rotation</param>
        public void UpdateMyDirection(double newDirection, double newViewportRotation, bool animated = true)
        {
            var newRotation = (int)(newDirection - newViewportRotation);
            var oldRotation = (int)((SymbolStyle)feature.Styles.First()).SymbolRotation;

            if (newRotation != oldRotation)
            {
                Direction = newDirection;

                // We have a direction update, so abort last animation
                if (mapView.AnimationIsRunning(animationMyDirectionName))
                    mapView.AbortAnimation(animationMyDirectionName);

                if (newRotation < 90 && oldRotation > 270)
                {
                    newRotation += 360;
                }
                else if (newRotation > 270 && oldRotation < 90)
                {
                    oldRotation += 360;
                }

                if (animated)
                {
                    var animation = new Animation((v) =>
                    {
                        if ((int)v != (int)((SymbolStyle)feature.Styles.First()).SymbolRotation)
                        {
                            ((SymbolStyle)feature.Styles.First()).SymbolRotation = (int)v % 360;
                            mapView.Refresh();
                        }
                    }, oldRotation, newRotation);

                    animation.Commit(mapView, animationMyDirectionName, 50, 500);
                }
                else
                {
                    ((SymbolStyle)feature.Styles.First()).SymbolRotation = newRotation % 360;
                    mapView.Refresh();
                }
            }
        }

        /// <summary>
        /// Updates my speed
        /// </summary>
        /// <param name="newSpeed">New speed</param>
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
    }
}