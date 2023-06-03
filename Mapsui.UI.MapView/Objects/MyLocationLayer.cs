using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Utilities;
using Animation = Mapsui.Animations.Animation;

#if __MAUI__
using Mapsui.UI.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
#else
using Mapsui.UI.Forms;
using Xamarin.Forms;
#endif

namespace Mapsui.UI.Objects;

/// <summary>
/// A layer to display a symbol for own location
/// </summary>
/// <remarks>
/// There are two different symbols for own loaction: one is used when there isn't a change in position (still),
/// and one is used, if the position changes (moving).
/// </remarks>
public class MyLocationLayer : BaseLayer, IModifyFeatureLayer
{
    private readonly MapView _mapView;
    private readonly GeometryFeature _feature;
    private SymbolStyle _locStyle;  // style for the location indicator
    private SymbolStyle _dirStyle;  // style for the view-direction indicator
    private CalloutStyle _coStyle;  // style for the callout

    private static int _bitmapMovingId = -1;
    private static int _bitmapStillId = -1;
    private static int _bitmapDirId = -1;

    private Position _animationMyLocationStart;
    private Position _animationMyLocationEnd;

    private bool _isMoving;

    /// <summary>
    /// Should be moving arrow or round circle displayed
    /// </summary>
    public bool IsMoving
    {
        get => _isMoving;
        set
        {
            if (_isMoving != value)
            {
                _isMoving = value;
                _locStyle.BitmapId = _isMoving ? _bitmapMovingId : _bitmapStillId;
            }
        }
    }

    private Position _myLocation = new(0, 0);
    private readonly ConcurrentHashSet<AnimationEntry<MapView>> _animations = new ();
    private readonly List<IFeature> _features;
    private AnimationEntry<MapView>? _animationMyDirection;
    private AnimationEntry<MapView>? _animationMyViewDirection;
    private AnimationEntry<MapView>? _animationMyLocation;

    /// <summary>
    /// Position of location, that is displayed
    /// </summary>
    /// <value>Position of location</value>
    public Position MyLocation => _myLocation;

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
    /// The text that is displayed in the MyLocation callout
    /// (can contain line breaks).
    /// </summary>
    public string CalloutText
    {
        get => _coStyle.Title ?? "";
        set
        {
            _coStyle.Title = value;
            _mapView.Refresh();
        }
    }

    /// <summary>
    /// Show or hide a callout with further infos next to the MyLocation symbol.
    /// </summary>
    public bool ShowCallout
    {
        get => _coStyle.Enabled;
        set
        {
            _coStyle.Enabled = value;
            _mapView.Refresh();
        }
    }

    /// <summary>
    /// This event is triggered whenever the MyLocation symbol or label is clicked.
    /// </summary>
    public event EventHandler<DrawableClickedEventArgs>? Clicked;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Objects.MyLocationLayer"/> class
    /// with a starting location.
    /// </summary>
    /// <param name="view">MapView, to which this layer belongs</param>
    /// <param name="position">Position, where to start</param>
    public MyLocationLayer(MapView view, Position position) : this(view)
    {
        _myLocation = position;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Objects.MyLocationLayer"/> class.
    /// </summary>
    /// <param name="view">MapView, to which this layer belongs</param>
    public MyLocationLayer(MapView view)
    {
        _mapView = view ?? throw new ArgumentNullException("MapView shouldn't be null");

        Enabled = false;
        IsMapInfoLayer = true;

        if (_bitmapMovingId == -1)
        {
            var bitmapMoving = typeof(MyLocationLayer).LoadBitmapId(@"Images.MyLocationMoving.svg");
            // Register bitmap
            _bitmapMovingId = bitmapMoving;
        }

        if (_bitmapStillId == -1)
        {
            var bitmapStill = typeof(MyLocationLayer).LoadBitmapId(@"Images.MyLocationStill.svg");
            // Register bitmap
            _bitmapStillId = bitmapStill;
        }

        if (_bitmapDirId == -1)
        {
            var bitmapDir = typeof(MyLocationLayer).LoadBitmapId(@"Images.MyLocationDir.svg");
            // Register bitmap
            _bitmapDirId = bitmapDir;
        }

        _feature = new GeometryFeature
        {
            Geometry = _myLocation.ToMapsui().ToPoint(),
            ["Label"] = "MyLocation moving",
        };

        _locStyle = new SymbolStyle
        {
            Enabled = true,
            BitmapId = _bitmapStillId,
            SymbolScale = Scale,
            SymbolRotation = Direction,
            SymbolOffset = new Offset(0, 0),
            Opacity = 1,
        };
        _dirStyle = new SymbolStyle
        {
            Enabled = false,
            BitmapId = _bitmapDirId,
            SymbolScale = 0.2,
            SymbolRotation = 0,
            SymbolOffset = new Offset(0, 0),
            Opacity = 1,
        };
        _coStyle = new CalloutStyle
        {
            Enabled = false,
            Type = CalloutType.Single,
            Title = "",
            TitleFontColor = Styles.Color.Black,
            ArrowAlignment = ArrowAlignment.Top,
            ArrowPosition = 0,
            SymbolOffset = new Offset(0, -SymbolStyle.DefaultHeight * 0.4f),
            MaxWidth = 300,
            RotateWithMap = true,
            SymbolOffsetRotatesWithMap = true,
            Color = Styles.Color.White,
            StrokeWidth = 0,
            ShadowWidth = 0
        };

        _feature.Styles.Clear();
        _feature.Styles.Add(_dirStyle);
        _feature.Styles.Add(_locStyle);
        _feature.Styles.Add(_coStyle);

        _features = new List<IFeature> { _feature };
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
            // We have a direction update, so abort last animation
            if (_animationMyLocation != null)
            {
                Animation.Stop(_mapView, _animationMyLocation, callFinal: true);
                _animations.TryRemove(_animationMyLocation);
                _animationMyLocation = null;
            }

            if (animated)
            {
                // Save values for new animation
                _animationMyLocationStart = MyLocation;
                _animationMyLocationEnd = newLocation;
                var deltaLat = _animationMyLocationEnd.Latitude - _animationMyLocationStart.Latitude;
                var deltaLon = _animationMyLocationEnd.Longitude - _animationMyLocationStart.Longitude;

                if (_mapView.Map.Navigator.Viewport.ToExtent() is not null)
                {
                    var fetchInfo = new FetchInfo(_mapView.Map.Navigator.Viewport.ToSection(), _mapView.Map?.CRS,
                        ChangeType.Discrete);
                    _animationMyLocation = new AnimationEntry<MapView>(
                        _animationMyLocationStart,
                        _animationMyLocationEnd,
                        animationStart: 0,
                        animationEnd: 1,
                        tick: (mapView, entry, v) =>
                        {
                            var modified = InternalUpdateMyLocation(new Position(
                                _animationMyLocationStart.Latitude + deltaLat * v,
                                _animationMyLocationStart.Longitude + deltaLon * v));
                            // Update viewport
                            if (modified && mapView.MyLocationFollow && mapView.MyLocationEnabled)
                                mapView.Map.Navigator.CenterOn(MyLocation.ToMapsui());
                            // Refresh map
                            if (mapView.MyLocationEnabled && modified)
                                mapView.Refresh();
                            return new AnimationResult<MapView>(mapView, true);
                        },
                        final: (mapView, entry) =>
                        {
                            mapView.Map.RefreshData(fetchInfo);
                            if (MyLocation != _animationMyLocationEnd)
                            {
                                InternalUpdateMyLocation(_animationMyLocationEnd);

                                if (mapView.MyLocationFollow && mapView.MyLocationEnabled)
                                    mapView.Map.Navigator.CenterOn(MyLocation.ToMapsui());

                                // Refresh map
                                if (mapView.MyLocationEnabled)
                                    mapView.Refresh();
                            }
                            
                            return new AnimationResult<MapView>(mapView, false);
                        });

                    Animation.Start(_animationMyLocation, 1000);
                    _animations.Add(_animationMyLocation);
                }
            }
            else
            {
                var modified = InternalUpdateMyLocation(newLocation);
                // Update viewport
                if (modified && _mapView.MyLocationFollow && _mapView.MyLocationEnabled)
                    _mapView.Map.Navigator.CenterOn(MyLocation.ToMapsui());
                // Refresh map
                if (_mapView.MyLocationEnabled && modified)
                    _mapView.Refresh();
            }
        }
    }

    public override bool UpdateAnimations()
    {
        if (_animations.Count > 0)
        {
            var animation = Animation.UpdateAnimations(_mapView, _animations);
            return animation.IsRunning;
        }

        return base.UpdateAnimations();
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _features;
    }

    /// <summary>
    /// Updates my movement direction
    /// </summary>
    /// <param name="newDirection">New direction</param>
    /// <param name="newViewportRotation">New viewport rotation</param>
    public void UpdateMyDirection(double newDirection, double newViewportRotation, bool animated = false)
    {
        var newRotation = (int)(newDirection - newViewportRotation);
        var oldRotation = (int)_locStyle.SymbolRotation;
        var diffRotation = newDirection - oldRotation;

        if (newRotation != oldRotation)
        {
            Direction = newDirection;

            // We have a direction update, so abort last animation
            if (_animationMyDirection != null)
            {
                Animation.Stop(_mapView, _animationMyDirection, callFinal: true);
                _animations.TryRemove(_animationMyDirection);
                _animationMyDirection = null;
            }

            if (newRotation < 90 && oldRotation > 270)
            {
                newRotation += 360;
            }
            else if (newRotation > 270 && oldRotation < 90)
            {
                oldRotation += 360;
            }

            var endRotation = newRotation % 360;

            if (animated)
            {
                _animationMyDirection = new AnimationEntry<MapView>(
                    oldRotation,
                    newRotation,
                    animationStart: 0,
                    animationEnd: 1,
                    tick: (mapView, entry, v) =>
                    {
                        var symbolRotation = (oldRotation + (int)(v * diffRotation)) % 360;
                        if ((int)symbolRotation != (int)_locStyle.SymbolRotation)
                        {
                            _locStyle.SymbolRotation = symbolRotation;
                            mapView.Refresh();
                        }

                        return new AnimationResult<MapView>(mapView, true);
                    },
                    final: (mapView, v) =>
                    {
                        if ((int)_locStyle.SymbolRotation != (int)endRotation)
                        {
                            _locStyle.SymbolRotation = endRotation;
                            mapView.Refresh();
                        }
                      
                        return new AnimationResult<MapView>(mapView, false);
                    });

                Animation.Start(_animationMyDirection, 1000);
                _animations.Add(_animationMyDirection);
            }
            else
            {
                _locStyle.SymbolRotation = endRotation;
                _mapView.Refresh();
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
            _mapView.Refresh();
    }

    /// <summary>
    /// Updates my view direction
    /// </summary>
    /// <param name="newDirection">New direction</param>
    /// <param name="newViewportRotation">New viewport rotation</param>
    /// <param name="animated">true if animated</param>
    public void UpdateMyViewDirection(double newDirection, double newViewportRotation, bool animated = false)
    {
        var newRotation = (int)(newDirection - newViewportRotation);
        var oldRotation = (int)_dirStyle.SymbolRotation;
        var diffRotation = newDirection - oldRotation;

        if (newRotation == -1.0)
        {
            // disable bitmap
            _dirStyle.Enabled = false;
        }
        else if (newRotation != oldRotation)
        {
            // We have a direction update, so abort last animation
            if (_animationMyViewDirection != null)
            {
                Animation.Stop(_mapView, _animationMyViewDirection, callFinal: true);
                _animations.TryRemove(_animationMyViewDirection);
                _animationMyViewDirection = null;
            }

            _dirStyle.Enabled = true;
            ViewingDirection = newDirection;

            if (newRotation < 90 && oldRotation > 270)
            {
                newRotation += 360;
            }
            else if (newRotation > 270 && oldRotation < 90)
            {
                oldRotation += 360;
            }

            var endRotation = newRotation % 360;

            if (animated)
            {
                _animationMyViewDirection = new AnimationEntry<MapView>(
                    oldRotation,
                    newRotation,
                    animationStart: 0,
                    animationEnd: 1,
                    tick: (mapView, entry, v) =>
                    {
                        var symbolRotation = (oldRotation + (int)(v * diffRotation)) % 360;
                        if ((int)symbolRotation != (int)_dirStyle.SymbolRotation)
                        {
                            _dirStyle.SymbolRotation = symbolRotation;
                            mapView.Refresh();
                        }

                        return new AnimationResult<MapView>(mapView, true);
                    },
                    final: (mapView, v) =>
                    {
                        if ((int)_dirStyle.SymbolRotation != endRotation)
                        {
                            _dirStyle.SymbolRotation = endRotation;
                            mapView.Refresh();
                        }

                        return new AnimationResult<MapView>(mapView, false);
                    });

                Animation.Start(_animationMyViewDirection, 1000);
                _animations.Add(_animationMyViewDirection);
            }
            else
            {
                _dirStyle.SymbolRotation = endRotation;
                _mapView.Refresh();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _feature.Dispose();
        }

        base.Dispose(disposing);
    }

    private bool InternalUpdateMyLocation(Position newLocation)
    {
        var modified = false;

        if (!_myLocation.Equals(newLocation))
        {
            _myLocation = newLocation;
            _feature.Geometry = _myLocation.ToPoint();
            modified = true;
        }

        return modified;
    }

    internal void HandleClicked(DrawableClickedEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
}
