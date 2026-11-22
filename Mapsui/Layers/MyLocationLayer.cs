using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using Animation = Mapsui.Animations.Animation;

namespace Mapsui.Layers;

/// <summary>
/// A layer to display a symbol for own location
/// </summary>
/// <remarks>
/// There are two different symbols for own location: one is used when there isn't a change in position (still),
/// and one is used, if the position changes (moving).
/// </remarks>
public class MyLocationLayer : BaseLayer, IDisposable
{
    private readonly Map _map;
    private readonly PointFeature _feature;
    private readonly ImageStyle _locStyle;  // style for the location indicator
    private readonly ImageStyle _dirStyle;  // style for the view-direction indicator
    private readonly CalloutStyle _coStyle;  // style for the callout

    private const string _movingImageSource = "embedded://Mapsui.Resources.Images.MyLocationMoving.svg";
    private const string _stillImageSource = "embedded://Mapsui.Resources.Images.MyLocationStill.svg";
    private const string _directionImageSource = "embedded://Mapsui.Resources.Images.MyLocationDir.svg";

    // Each layer gets its own Image instances because Image is mutable (SvgFillColor etc.), so changing
    // one layer's image must not affect another layer.
    private Image _stillImage = _stillImageSource;
    private Image _movingImage = _movingImageSource;
    private Image _directionImage = _directionImageSource;

    private MPoint? _animationStart;
    private MPoint? _animationEnd;

    private readonly ConcurrentHashSet<AnimationEntry<Map>> _animations = [];
    private readonly List<IFeature> _features;
    private AnimationEntry<Map>? _animationMyDirection;
    private AnimationEntry<Map>? _animationMyViewDirection;
    private AnimationEntry<Map>? _animationMyLocation;

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
                _locStyle.Image = _isMoving ? _movingImage : _stillImage;
            }
        }
    }

    private bool _isCentered = true;

    /// <summary>
    /// MyLocation is always in the center of the map
    /// </summary>
    public bool IsCentered
    {
        get => _isCentered;
        set
        {
            if (_isCentered != value)
            {
                _isCentered = value;
            }
        }
    }

    private MPoint _myLocation = new(0, 0);

    /// <summary>
    /// Position of location, that is displayed
    /// </summary>
    /// <value>Position of location</value>
    public MPoint MyLocation => _myLocation;

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
    /// Scale of the location symbol (the <see cref="StillImage"/> or <see cref="MovingImage"/>).
    /// </summary>
    /// <value>Scale of symbol</value>
    public double Scale
    {
        get => _locStyle.SymbolScale;
        set
        {
            _locStyle.SymbolScale = value;
            _map.RefreshGraphics(); // The symbol size changes, so the old extent is unknown to a partial refresh.
        }
    }

    /// <summary>
    /// Scale of the viewing direction symbol (the <see cref="DirectionImage"/>). The default image is large
    /// (400 pixels), so the default scale is 0.2. Adjust this when using a custom <see cref="DirectionImage"/>.
    /// </summary>
    public double DirectionScale
    {
        get => _dirStyle.SymbolScale;
        set
        {
            _dirStyle.SymbolScale = value;
            _map.RefreshGraphics();
        }
    }

    /// <summary>
    /// Image of the location symbol when the location does not change (<see cref="IsMoving"/> is false).
    /// Like the <see cref="MovingImage"/> it is rotated to the movement <see cref="Direction"/>, so the image
    /// should point up (north) when not rotated. The default is a blue circle. Use <see cref="Scale"/> to change its size.
    /// </summary>
    /// <remarks>
    /// Changing a property of the assigned image, such as <see cref="Image.SvgFillColor"/>, does not redraw the map.
    /// Assign a new image instead.
    /// </remarks>
    public Image StillImage
    {
        get => _stillImage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _stillImage = value;
            if (!_isMoving) SetLocationImage(value);
        }
    }

    /// <summary>
    /// Image of the location symbol when the location changes (<see cref="IsMoving"/> is true). It is rotated
    /// to the movement <see cref="Direction"/>, so the image should point up (north) when not rotated.
    /// The default is a blue arrow. Use <see cref="Scale"/> to change its size.
    /// </summary>
    public Image MovingImage
    {
        get => _movingImage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _movingImage = value;
            if (_isMoving) SetLocationImage(value);
        }
    }

    /// <summary>
    /// Image of the viewing direction indicator, drawn below the location symbol and rotated to the
    /// <see cref="ViewingDirection"/>, so the image should point up (north) when not rotated.
    /// The default is a blue cone. Use <see cref="DirectionScale"/> to change its size.
    /// </summary>
    public Image DirectionImage
    {
        get => _directionImage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _directionImage = value;
            _dirStyle.Image = value;
            _map.RefreshGraphics();
        }
    }

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
            _map.RefreshGraphics(); // Callout size is variable so full refresh is needed.
        }
    }

    /// <summary>
    /// Show or hide a callout with further info next to the MyLocation symbol.
    /// </summary>
    public bool ShowCallout
    {
        get => _coStyle.Enabled;
        set
        {
            _coStyle.Enabled = value;
            _map.RefreshGraphics(); // Callout size is variable so full refresh is needed.
        }
    }

    /// <summary>
    /// This event is triggered whenever the MyLocation symbol or label is clicked.
    /// </summary>
    public event EventHandler<MapEventArgs>? Tapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.Layers.MyLocationLayer"/> class
    /// with a starting location.
    /// </summary>
    /// <param name="map">MapView, to which this layer belongs</param>
    /// <param name="location">Location, where to start</param>
    public MyLocationLayer(Map map, MPoint location) : this(map)
    {
        _myLocation = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.Layers.MyLocationLayer"/> class.
    /// </summary>
    /// <param name="map">Map, to which this layer belongs</param>
    public MyLocationLayer(Map map)
    {
        ArgumentNullException.ThrowIfNull(map);

        _map = map;
        _map.Tapped += MapTapped;

        Enabled = true;

        _feature = new PointFeature(_myLocation)
        {
            ["Label"] = "MyLocation",
        };

        _locStyle = new ImageStyle
        {
            Enabled = true,
            Image = _stillImage,
            SymbolScale = 1.0,
            SymbolRotation = Direction,
            Offset = new Offset(0, 0),
            Opacity = 1,
        };

        _dirStyle = new ImageStyle
        {
            Enabled = false,
            Image = _directionImage,
            SymbolScale = 0.2,
            SymbolRotation = 0,
            Offset = new Offset(0, 0),
            Opacity = 1,
        };

        _coStyle = new CalloutStyle
        {
            Enabled = false,
            Type = CalloutType.Single,
            Title = "",
            TitleFontColor = Color.Black,
            MaxWidth = 300,
            RotateWithMap = true,
            SymbolOffsetRotatesWithMap = true,
            Offset = new Offset(0, -SymbolStyle.DefaultHeight * 0.4f),
            BalloonDefinition = new CalloutBalloonDefinition
            {
                Color = Color.White,
                TailAlignment = TailAlignment.Top,
                TailPosition = 0,
                StrokeWidth = 0,
                ShadowWidth = 0
            },
        };

        _feature.Styles.Clear();
        _feature.Styles.Add(_dirStyle);
        _feature.Styles.Add(_locStyle);
        _feature.Styles.Add(_coStyle);

        _features = [_feature];
        Style = null;
    }

    /// <summary>
    /// Updates own location
    /// </summary>
    /// <param name="newLocation">New location</param>
    public void UpdateMyLocation(MPoint newLocation, bool animated = false)
    {
        if (!MyLocation.Equals(newLocation))
        {
            // We have a location update, so abort last animation
            if (_animationMyLocation != null)
            {
                Animation.Stop(_map, _animationMyLocation, callFinal: true);
                _animations.TryRemove(_animationMyLocation);
                _animationMyLocation = null;
            }

            if (animated)
            {
                // Save values for new animation
                _animationStart = MyLocation;
                _animationEnd = newLocation;
                var deltaX = _animationEnd.X - _animationStart.X;
                var deltaY = _animationEnd.Y - _animationStart.Y;

                if (_map.Navigator.Viewport.ToExtent() is not null)
                {
                    // Refresh the end viewport at the start of the animation so it has time to load.
                    var endViewport = _map.Navigator.Viewport with { CenterX = _animationEnd.X, CenterY = _animationEnd.Y };
                    _map.RefreshData(endViewport);

                    _animationMyLocation = new AnimationEntry<Map>(
                        MyLocation,
                        newLocation,
                        animationStart: 0,
                        animationEnd: 1,
                        tick: (map, entry, v) =>
                        {
                            var dirtyRect = InternalUpdateMyLocation(new MPoint(_animationStart.X + deltaX * v, _animationStart.Y + deltaY * v));
                            if (dirtyRect is not null) map.RefreshGraphics(dirtyRect);
                            return new AnimationResult<Map>(map, true);
                        },
                        final: (map, entry) =>
                        {
                            if (!MyLocation.Equals(_animationEnd))
                            {
                                var dirtyRect = InternalUpdateMyLocation(_animationEnd);
                                if (dirtyRect is not null) map.RefreshGraphics(dirtyRect);
                            }

                            return new AnimationResult<Map>(map, false);
                        });

                    Animation.Start(_animationMyLocation, 1000);
                    _animations.Add(_animationMyLocation);

                    // Update viewport
                    if (_isCentered)
                    {
                        _map?.Navigator.CenterOn(_animationEnd, 1000, Easing.Linear);
                    }
                }
            }
            else
            {
                var dirtyRect = InternalUpdateMyLocation(newLocation);
                if (dirtyRect is not null) _map.RefreshGraphics(dirtyRect);

                // Update viewport
                if (_isCentered)
                {
                    _map?.Navigator.CenterOn(_myLocation);
                }
            }
        }
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
                Animation.Stop(_map, _animationMyDirection, callFinal: true);
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
                _animationMyDirection = new AnimationEntry<Map>(
                    oldRotation,
                    newRotation,
                    animationStart: 0,
                    animationEnd: 1,
                    tick: (map, entry, v) =>
                    {
                        var symbolRotation = (oldRotation + (int)(v * diffRotation)) % 360;
                        if ((int)symbolRotation != (int)_locStyle.SymbolRotation)
                        {
                            _locStyle.SymbolRotation = symbolRotation;
                            map.RefreshGraphics(DirtyRectForSymbol(_myLocation));
                        }

                        return new AnimationResult<Map>(map, true);
                    },
                    final: (map, v) =>
                    {
                        if ((int)_locStyle.SymbolRotation != (int)endRotation)
                        {
                            _locStyle.SymbolRotation = endRotation;
                            map.RefreshGraphics(DirtyRectForSymbol(_myLocation));
                        }

                        return new AnimationResult<Map>(map, false);
                    });

                Animation.Start(_animationMyDirection, 1000);
                _animations.Add(_animationMyDirection);
            }
            else
            {
                _locStyle.SymbolRotation = endRotation;
                _map.RefreshGraphics(DirtyRectForSymbol(_myLocation));
            }
        }
    }

    /// <summary>
    /// Updates my speed
    /// </summary>
    /// <param name="newSpeed">New speed</param>
    public void UpdateMySpeed(double newSpeed)
    {
        var isMoving = newSpeed > 0;
        if (isMoving == IsMoving) return;

        // The still and moving images can differ in size, so clear the extent of the old image as well.
        var oldDirtyRect = DirtyRectForSymbol(_myLocation);
        IsMoving = isMoving;
        if (IsImageCached(_locStyle))
            _map.RefreshGraphics(oldDirtyRect.Join(DirtyRectForSymbol(_myLocation)));
        else
            _map.RefreshGraphics(); // The new image's size is unknown until it is drawn.
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
            if (_dirStyle.Enabled)
            {
                _dirStyle.Enabled = false;
                // A partial refresh would no longer include the direction image, so it would stay on screen.
                _map.RefreshGraphics();
            }
        }
        else if (newRotation != oldRotation)
        {
            // We have a direction update, so abort last animation
            if (_animationMyViewDirection != null)
            {
                Animation.Stop(_map, _animationMyViewDirection, callFinal: true);
                _animations.TryRemove(_animationMyViewDirection);
                _animationMyViewDirection = null;
            }

            if (!_dirStyle.Enabled)
            {
                _dirStyle.Enabled = true;
                // The direction image is not drawn while disabled, so its size may be unknown to a partial refresh.
                _map.RefreshGraphics();
            }

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
                _animationMyViewDirection = new AnimationEntry<Map>(
                    oldRotation,
                    newRotation,
                    animationStart: 0,
                    animationEnd: 1,
                    tick: (map, entry, v) =>
                    {
                        var symbolRotation = (oldRotation + (int)(v * diffRotation)) % 360;
                        if ((int)symbolRotation != (int)_dirStyle.SymbolRotation)
                        {
                            _dirStyle.SymbolRotation = symbolRotation;
                            map.RefreshGraphics(DirtyRectForSymbol(_myLocation));
                        }

                        return new AnimationResult<Map>(map, true);
                    },
                    final: (map, v) =>
                    {
                        if ((int)_dirStyle.SymbolRotation != endRotation)
                        {
                            _dirStyle.SymbolRotation = endRotation;
                            map.RefreshGraphics(DirtyRectForSymbol(_myLocation));
                        }

                        return new AnimationResult<Map>(map, false);
                    });

                Animation.Start(_animationMyViewDirection, 1000);
                _animations.Add(_animationMyViewDirection);
            }
            else
            {
                _dirStyle.SymbolRotation = endRotation;
                _map.RefreshGraphics(DirtyRectForSymbol(_myLocation));
            }
        }
    }

    public override bool UpdateAnimations()
    {
        if (_animations.Count > 0)
        {
            var animation = Animation.UpdateAnimations(_map, _animations);
            return animation.IsRunning;
        }

        return base.UpdateAnimations();
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _features;
    }

    private MRect? InternalUpdateMyLocation(MPoint newLocation)
    {
        if (_myLocation.Equals(newLocation)) return null;
        var oldLocation = _myLocation;
        _myLocation = newLocation;
        _feature.Modified();
        _feature.Point.X = _myLocation.X;
        _feature.Point.Y = _myLocation.Y;
        // Union old and new symbol extents so the old symbol position is also cleared.
        return DirtyRectForSymbol(oldLocation).Join(DirtyRectForSymbol(newLocation));
    }

    private void SetLocationImage(Image image)
    {
        _locStyle.Image = image;
        _map.RefreshGraphics(); // The new image may differ in size, so the old extent is unknown to a partial refresh.
    }

    private MRect DirtyRectForSymbol(MPoint location)
    {
        // Full symbol size used as the half-extent on each side — a conservative world-space bound
        // that covers the rendered pixel footprint at any rotation, regardless of sub-pixel offsets or anti-aliasing.
        var symbolSize = GetSymbolSize(_locStyle);
        if (_dirStyle.Enabled)
            symbolSize = Math.Max(symbolSize, GetSymbolSize(_dirStyle));
        var halfWorld = symbolSize * _map.Navigator.Viewport.Resolution;
        return new MRect(location.X - halfWorld, location.Y - halfWorld, location.X + halfWorld, location.Y + halfWorld);
    }

    private double GetSymbolSize(ImageStyle style)
    {
        // Custom images can be any size, so use the size of the image the renderer has already drawn.
        // This lookup never creates an image; rendering belongs in the renderer. An image that is not drawn
        // yet falls back to the default size, so code that shows a new image requests a full refresh instead.
        var image = GetCachedImage(style);
        var size = image is null ? SymbolStyle.DefaultWidth : Math.Max(image.Width, image.Height);
        return size * style.SymbolScale;
    }

    private bool IsImageCached(ImageStyle style) => GetCachedImage(style) is not null;

    private IDrawableImage? GetCachedImage(ImageStyle style) =>
        style.Image is null ? null : _map.RenderService.DrawableImageCache.GetIfCreated(style.Image.SourceId);

    private void MapTapped(object? s, MapEventArgs e)
    {
        var mapInfo = e.GetMapInfo([this]);
        if (mapInfo.Feature != null && mapInfo.Feature.Equals(_feature))
        {
            Tapped?.Invoke(this, e);
            e.Handled = true;
        }
    }
}
