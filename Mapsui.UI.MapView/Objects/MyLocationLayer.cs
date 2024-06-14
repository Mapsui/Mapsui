﻿using Mapsui.Layers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Utilities;
using Animation = Mapsui.Animations.Animation;
using Mapsui.UI.Maui;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Objects;

/// <summary>
/// A layer to display a symbol for own location
/// </summary>
/// <remarks>
/// There are two different symbols for own location: one is used when there isn't a change in position (still),
/// and one is used, if the position changes (moving).
/// </remarks>
public class MyLocationLayer : BaseLayer
{
    private MapView _mapView;
    private readonly GeometryFeature _feature;
    private readonly SymbolStyle _locStyle;  // style for the location indicator
    private readonly SymbolStyle _dirStyle;  // style for the view-direction indicator
    private readonly CalloutStyle _coStyle;  // style for the callout

    private static readonly string _movingImageSource = "embedded://Mapsui.Resources.Images.MyLocationMoving.svg";
    private static readonly string _stillImageSource = "embedded://Mapsui.Resources.Images.MyLocationStill.svg";
    private static readonly string _directionImageSource = "embedded://Mapsui.Resources.Images.MyLocationDir.svg";

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
                _locStyle.ImageSource = _isMoving ? _movingImageSource : _stillImageSource;
            }
        }
    }


    private Position _myLocation = new(0, 0);
    private readonly ConcurrentHashSet<AnimationEntry<MapView>> _animations = new();
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

    /// <summary> Sets Map View </summary>
    internal MapView MapView
    {
        set => _mapView = value ?? throw new NullReferenceException();
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
    public MyLocationLayer(MapView view) : this()
    {
        _mapView = view ?? throw new ArgumentNullException("MapView shouldn't be null");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Objects.MyLocationLayer"/> class.
    /// </summary>
    internal MyLocationLayer()
    {
        _mapView = default!; // will be set in constructor with MapView
        Enabled = false;
        IsMapInfoLayer = true;

        _feature = new GeometryFeature
        {
            Geometry = _myLocation.ToMapsui().ToPoint(),
            ["Label"] = "MyLocation moving",
        };

        _locStyle = new SymbolStyle
        {
            Enabled = true,
            ImageSource = _stillImageSource,
            SymbolScale = Scale,
            SymbolRotation = Direction,
            SymbolOffset = new Offset(0, 0),
            Opacity = 1,
        };

        _dirStyle = new SymbolStyle
        {
            Enabled = false,
            ImageSource = _directionImageSource,
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
            TitleFontColor = Color.Black,
            SymbolOffset = new Offset(0, -SymbolStyle.DefaultHeight * 0.4f),
            MaxWidth = 300,
            RotateWithMap = true,
            SymbolOffsetRotatesWithMap = true,
            BalloonDefinition = new CalloutBalloonDefinition
            {
                TailAlignment = TailAlignment.Top,
                TailPosition = 0,
                Color = Color.White,
                StrokeWidth = 0,
                ShadowWidth = 0
            },
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
    /// <param name="animated">true if there is an animation from the old to the new location.</param>
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
    /// <param name="animated">true if animated</param>
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

    private bool InternalUpdateMyLocation(Position newLocation)
    {
        var modified = false;

        if (!_myLocation.Equals(newLocation))
        {
            _myLocation = newLocation;
            _feature.Geometry = _myLocation.ToPoint();
            _feature.Modified();
            modified = true;
        }

        return modified;
    }

    internal void HandleClicked(DrawableClickedEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
}
