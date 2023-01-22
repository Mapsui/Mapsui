using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

using System;
using System.Collections.Generic;
using System.Linq;

using Mapsui.Extensions;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;

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

namespace Mapsui.UI.Objects;

/// <summary>
/// A layer to display a symbol for own location
/// </summary>
/// <remarks>
/// There are two different symbols for own loaction: one is used when there isn't a change in position (still),
/// and one is used, if the position changes (moving).
/// </remarks>
public class MyLocationLayer : MemoryLayer
{
    private readonly MapView _mapView;
    private readonly GeometryFeature _feature;
    private SymbolStyle _locStyle;  // style for the location indicator
    private SymbolStyle _dirStyle;  // style for the view-direction indicator
    private CalloutStyle _coStyle;  // style for the callout

    private static int _bitmapMovingId = -1;
    private static int _bitmapStillId = -1;
    private static int _bitmapDirId = -1;

    private const string AnimationMyLocationName = "animationMyLocationPosition";
    private const string AnimationMyDirectionName = "animationMyDirectionPosition";
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
        myLocation = position;
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
            Geometry = myLocation.ToMapsui().ToPoint(),
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

        Features = new List<IFeature> { _feature };
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
            if (_mapView.AnimationIsRunning(AnimationMyLocationName))
                _mapView.AbortAnimation(AnimationMyLocationName);

            if (animated)
            {
                // Save values for new animation
                _animationMyLocationStart = MyLocation;
                _animationMyLocationEnd = newLocation;

                var animation = new Animation((v) =>
                {
                    var deltaLat = (_animationMyLocationEnd.Latitude - _animationMyLocationStart.Latitude) * v;
                    var deltaLon = (_animationMyLocationEnd.Longitude - _animationMyLocationStart.Longitude) * v;
                    var modified = InternalUpdateMyLocation(new Position(_animationMyLocationStart.Latitude + deltaLat, _animationMyLocationStart.Longitude + deltaLon));
                    // Update viewport
                    if (modified && _mapView.MyLocationFollow && _mapView.MyLocationEnabled)
                        _mapView.Navigator?.CenterOn(MyLocation.ToMapsui());
                    // Refresh map
                    if (_mapView.MyLocationEnabled && modified)
                        _mapView.Refresh();
                }, 0.0, 1.0);

                if (_mapView.Viewport.Extent != null)
                {
                    var fetchInfo = new FetchInfo(_mapView.Viewport.Extent, _mapView.Viewport.Resolution, _mapView.Map?.CRS, ChangeType.Discrete);
                    // At the end, update viewport
                    animation.Commit(_mapView, AnimationMyLocationName, 100, 3000, finished: (s, v) => _mapView.Map?.RefreshData(fetchInfo));
                }
            }
            else
            {
                var modified = InternalUpdateMyLocation(newLocation);
                // Update viewport
                if (modified && _mapView.MyLocationFollow && _mapView.MyLocationEnabled)
                    _mapView.Navigator?.CenterOn(MyLocation.ToMapsui());
                // Refresh map
                if (_mapView.MyLocationEnabled && modified)
                    _mapView.Refresh();
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
        var oldRotation = (int)_locStyle.SymbolRotation;

        if (newRotation != oldRotation)
        {
            Direction = newDirection;

            // We have a direction update, so abort last animation
            if (_mapView.AnimationIsRunning(AnimationMyDirectionName))
                _mapView.AbortAnimation(AnimationMyDirectionName);

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
                    if ((int)v != (int)_locStyle.SymbolRotation)
                    {
                        _locStyle.SymbolRotation = (int)v % 360;
                        _mapView.Refresh();
                    }
                }, oldRotation, newRotation);

                animation.Commit(_mapView, AnimationMyDirectionName, 50, 500);
            }
            else
            {
                _locStyle.SymbolRotation = newRotation % 360;
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
    public void UpdateMyViewDirection(double newDirection, double newViewportRotation, bool animated = false)
    {
        var newRotation = (int)(newDirection - newViewportRotation);
        var oldRotation = (int)_dirStyle.SymbolRotation;

        if (newRotation == -1.0)
        {
            // disable bitmap
            _dirStyle.Enabled = false;
        }
        else if (newRotation != oldRotation)
        {
            _dirStyle.Enabled = true;
            ViewingDirection = newDirection;

            // We have a direction update, so abort last animation
            if (_mapView.AnimationIsRunning(AnimationMyDirectionName))
                _mapView.AbortAnimation(AnimationMyDirectionName);

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
                    if ((int)v != (int)_dirStyle.SymbolRotation)
                    {
                        _dirStyle.SymbolRotation = (int)v % 360;
                        _mapView.Refresh();
                    }
                }, oldRotation, newRotation);

                animation.Commit(_mapView, AnimationMyDirectionName, 50, 500);
            }
            else
            {
                _dirStyle.SymbolRotation = newRotation % 360;
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

        if (!myLocation.Equals(newLocation))
        {
            myLocation = newLocation;
            _feature.Geometry = myLocation.ToPoint();
            modified = true;
        }

        return modified;
    }

    internal void HandleClicked(DrawableClickedEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
}
