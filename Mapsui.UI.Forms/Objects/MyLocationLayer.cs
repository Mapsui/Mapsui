﻿using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Mapsui.Extensions;
using Mapsui.GeometryLayers;

#if __MAUI__
using Mapsui.UI.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Animation = Microsoft.Maui.Controls.Animation;
#else
using Mapsui.UI.Forms;
using Xamarin.Forms;
using Animation = Xamarin.Forms.Animation;
#endif

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
        private readonly MapView mapView;
        private readonly GeometryFeature feature;
        private readonly GeometryFeature featureDir;

        private static int bitmapMovingId = -1;
        private static int bitmapStillId = -1;
        private static int bitmapDirId = -1;

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
            get => isMoving;
            set
            {
                if (isMoving != value)
                {
                    isMoving = value;
                    ((SymbolStyle)feature.Styles.First()).BitmapId = isMoving ? bitmapMovingId : bitmapStillId;
                }
            }
        }

        private Position myLocation = new(0, 0);

        /// <summary>
        /// Position of location, that is displayed
        /// </summary>
        /// <value>Position of location</value>
        public Position MyLocation => myLocation;

        /// <summary>
        /// Movement direction of device at location
        /// </summary>
        /// <value>Direction at location</value>
        public double Direction { get; private set; } = 0.0;

        /// <summary>
        /// Viewing direction of device (in degrees wrt. north direction)
        /// </summary>
        /// <value>Direction at location</value>
        public double ViewingDirection { get; private set; } = -1.0;

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
            mapView = view ?? throw new ArgumentNullException("MapView shouldn't be null");

            Enabled = false;

            if (bitmapMovingId == -1)
            {
                var bitmapMoving = typeof(MyLocationLayer).LoadBitmapId(@"Images.MyLocationMoving.svg");
                // Register bitmap
                bitmapMovingId = bitmapMoving;
            }

            if (bitmapStillId == -1)
            {
                var bitmapStill = typeof(MyLocationLayer).LoadBitmapId(@"Images.MyLocationStill.svg");
                // Register bitmap
                bitmapStillId = bitmapStill;
            }

            if (bitmapDirId == -1)
            {
                var bitmapDir = typeof(MyLocationLayer).LoadBitmapId(@"Images.MyLocationDir.svg");
                // Register bitmap
                bitmapDirId = bitmapDir;
            }

            feature = new GeometryFeature
            {
                Geometry = myLocation.ToMapsui().ToPoint(),
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

            featureDir = new GeometryFeature
            {
                Geometry = myLocation.ToMapsui().ToPoint(),
                ["Label"] = "My view direction",
            };

            featureDir.Styles.Clear();
            featureDir.Styles.Add(new SymbolStyle
            {
                Enabled = false,
                BitmapId = bitmapDirId,
                SymbolScale = 0.2,
                SymbolRotation = 0,
                SymbolOffset = new Offset(0, 0),
                Opacity = 1,
            });

            DataSource = new MemoryProvider<IFeature>(new List<IFeature> { featureDir, feature });
            Style = null;
        }

        /// <summary>
        /// Updates my location
        /// </summary>
        /// <param name="newLocation">New location</param>
        public void UpdateMyLocation(Position newLocation, bool animated = false)
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

                    var animation = new Animation((v) => {
                        var deltaLat = (animationMyLocationEnd.Latitude - animationMyLocationStart.Latitude) * v;
                        var deltaLon = (animationMyLocationEnd.Longitude - animationMyLocationStart.Longitude) * v;
                        var modified = InternalUpdateMyLocation(new Position(animationMyLocationStart.Latitude + deltaLat, animationMyLocationStart.Longitude + deltaLon));
                        // Update viewport
                        if (modified && mapView.MyLocationFollow && mapView.MyLocationEnabled)
                            mapView.Navigator.CenterOn(MyLocation.ToMapsui());
                        // Refresh map
                        if (mapView.MyLocationEnabled && modified)
                            mapView.Refresh();
                    }, 0.0, 1.0);

                    if (mapView.Viewport.Extent != null)
                    {
                        var fetchInfo = new FetchInfo(mapView.Viewport.Extent, mapView.Viewport.Resolution, mapView.Map?.CRS, ChangeType.Discrete);
                        // At the end, update viewport
                        animation.Commit(mapView, animationMyLocationName, 100, 3000, finished: (s, v) => mapView.Map?.RefreshData(fetchInfo));
                    }
                }
                else
                {
                    var modified = InternalUpdateMyLocation(newLocation);
                    // Update viewport
                    if (modified && mapView.MyLocationFollow && mapView.MyLocationEnabled)
                        mapView.Navigator.CenterOn(MyLocation.ToMapsui());
                    // Refresh map
                    if (mapView.MyLocationEnabled && modified)
                        mapView.Refresh();
                }
            }
        }

        /// <summary>
        /// Updates my movement direction
        /// </summary>
        /// <param name="newDirection">New direction</param>
        /// <param name="newViewportRotation">New viewport rotation</param>
        public void UpdateMyDirection(double newDirection, double newViewportRotation, bool animated = false)
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
                    var animation = new Animation((v) => {
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

        /// <summary>
        /// Updates my view direction
        /// </summary>
        /// <param name="newDirection">New direction</param>
        /// <param name="newViewportRotation">New viewport rotation</param>
        public void UpdateMyViewDirection(double newDirection, double newViewportRotation, bool animated = false)
        {
            var newRotation = (int)(newDirection - newViewportRotation);
            var oldRotation = (int)((SymbolStyle)featureDir.Styles.First()).SymbolRotation;

            if (newRotation == -1.0)
            {
                // disable bitmap
                ((SymbolStyle)featureDir.Styles.First()).Enabled = false;
            }
            else if (newRotation != oldRotation)
            {
                ((SymbolStyle)featureDir.Styles.First()).Enabled = true;
                ViewingDirection = newDirection;

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
                    var animation = new Animation((v) => {
                        if ((int)v != (int)((SymbolStyle)featureDir.Styles.First()).SymbolRotation)
                        {
                            ((SymbolStyle)featureDir.Styles.First()).SymbolRotation = (int)v % 360;
                            mapView.Refresh();
                        }
                    }, oldRotation, newRotation);

                    animation.Commit(mapView, animationMyDirectionName, 50, 500);
                }
                else
                {
                    ((SymbolStyle)featureDir.Styles.First()).SymbolRotation = newRotation % 360;
                    mapView.Refresh();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.feature.Dispose();
                this.featureDir.Dispose();
            }

            base.Dispose(disposing);
        }

        private bool InternalUpdateMyLocation(Position newLocation)
        {
            var modified = false;

            if (!myLocation.Equals(newLocation))
            {
                myLocation = newLocation;
                feature.Geometry = myLocation.ToPoint();
                featureDir.Geometry = myLocation.ToPoint();
                modified = true;
            }

            return modified;
        }
    }
}