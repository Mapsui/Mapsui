using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;

namespace Mapsui.Layers;

/// <summary>
/// A layer to display a symbol for own location
/// </summary>
/// <remarks>
/// There are two different symbols for own location: one is used when there isn't a change in position (still),
/// and one is used, if the position changes (moving).
/// <para>
/// The members that change what is shown can be called from any thread, for example from a GPS callback. They
/// describe the requested state in an immutable record, and the render pipeline applies it in
/// <see cref="UpdateAnimations"/>, which is the only code that changes the styles and the feature. See the
/// threading section in docs/general/markdown/mapsui-architecture-principles.md.
/// </para>
/// </remarks>
public class MyLocationLayer : BaseLayer, IDisposable
{
    private const string _movingImageSource = "embedded://Mapsui.Resources.Images.MyLocationMoving.svg";
    private const string _stillImageSource = "embedded://Mapsui.Resources.Images.MyLocationStill.svg";
    private const string _directionImageSource = "embedded://Mapsui.Resources.Images.MyLocationDir.svg";
    private const long _animationDuration = 1000;

    /// <summary>
    /// Everything a caller can request. A negative <see cref="ViewingDirection"/> hides the direction image.
    /// </summary>
    private sealed record State(
        MPoint Location,
        bool AnimateLocation,
        double Direction,
        double DirectionViewportRotation,
        bool AnimateDirection,
        double ViewingDirection,
        double ViewingDirectionViewportRotation,
        bool AnimateViewingDirection,
        bool IsMoving,
        bool IsCentered,
        Image StillImage,
        Image MovingImage,
        Image DirectionImage,
        double Scale,
        double DirectionScale,
        string CalloutText,
        bool ShowCallout)
    {
        public Image LocationImage => IsMoving ? MovingImage : StillImage;
        public bool ShowDirection => ViewingDirection >= 0;
        // The symbol rotation is relative to the viewport; the animation code compares whole degrees.
        public int LocationRotation => (int)(Direction - DirectionViewportRotation);
        public int DirectionRotation => (int)(ViewingDirection - ViewingDirectionViewportRotation);
    }

    private readonly Map _map;
    private readonly PointFeature _feature;
    private readonly ImageStyle _locStyle;  // style for the location indicator
    private readonly ImageStyle _dirStyle;  // style for the view-direction indicator
    private readonly CalloutStyle _coStyle;  // style for the callout
    private readonly List<IFeature> _features;
    private readonly StateHandover<State> _state;

    // Everything below is owned by the render pipeline and only touched from UpdateAnimations.
    private readonly AnimationList<Map> _animations = new();
    private AnimationEntry<Map>? _animationMyDirection;
    private AnimationEntry<Map>? _animationMyViewDirection;
    private AnimationEntry<Map>? _animationMyLocation;
    private bool _hasLocation; // False until a location was set; the first location is never animated from (0, 0).

    /// <summary>
    /// Should be moving arrow or round circle displayed
    /// </summary>
    public bool IsMoving
    {
        get => _state.Latest.IsMoving;
        set => Publish(s => s.IsMoving == value ? s : s with { IsMoving = value });
    }

    /// <summary>
    /// MyLocation is always in the center of the map
    /// </summary>
    public bool IsCentered
    {
        get => _state.Latest.IsCentered;
        set => Publish(s => s.IsCentered == value ? s : s with { IsCentered = value });
    }

    /// <summary>
    /// Position of location, that is displayed
    /// </summary>
    /// <value>Position of location</value>
    public MPoint MyLocation => new(_state.Latest.Location); // A copy, so a caller cannot change the published state.

    /// <summary>
    /// Movement direction of device at location
    /// </summary>
    /// <value>Direction at location</value>
    public double Direction => _state.Latest.Direction;

    /// <summary>
    /// Viewing direction of device (in degrees wrt. north direction). Negative when not shown.
    /// </summary>
    /// <value>Viewing direction, or -1 when the direction image is hidden</value>
    public double ViewingDirection => _state.Latest.ViewingDirection;

    /// <summary>
    /// Scale of the location symbol (the <see cref="StillImage"/> or <see cref="MovingImage"/>).
    /// </summary>
    /// <value>Scale of symbol</value>
    public double Scale
    {
        get => _state.Latest.Scale;
        set => Publish(s => s.Scale == value ? s : s with { Scale = value });
    }

    /// <summary>
    /// Scale of the viewing direction symbol (the <see cref="DirectionImage"/>). The default image is large
    /// (400 pixels), so the default scale is 0.2. Adjust this when using a custom <see cref="DirectionImage"/>.
    /// </summary>
    public double DirectionScale
    {
        get => _state.Latest.DirectionScale;
        set => Publish(s => s.DirectionScale == value ? s : s with { DirectionScale = value });
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
        get => _state.Latest.StillImage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Publish(s => s.StillImage == value ? s : s with { StillImage = value });
        }
    }

    /// <summary>
    /// Image of the location symbol when the location changes (<see cref="IsMoving"/> is true). It is rotated
    /// to the movement <see cref="Direction"/>, so the image should point up (north) when not rotated.
    /// The default is a blue arrow. Use <see cref="Scale"/> to change its size.
    /// </summary>
    public Image MovingImage
    {
        get => _state.Latest.MovingImage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Publish(s => s.MovingImage == value ? s : s with { MovingImage = value });
        }
    }

    /// <summary>
    /// Image of the viewing direction indicator, drawn below the location symbol and rotated to the
    /// <see cref="ViewingDirection"/>, so the image should point up (north) when not rotated.
    /// The default is a blue cone. Use <see cref="DirectionScale"/> to change its size.
    /// </summary>
    public Image DirectionImage
    {
        get => _state.Latest.DirectionImage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            Publish(s => s.DirectionImage == value ? s : s with { DirectionImage = value });
        }
    }

    /// <summary>
    /// The text that is displayed in the MyLocation callout
    /// (can contain line breaks).
    /// </summary>
    public string CalloutText
    {
        get => _state.Latest.CalloutText;
        set
        {
            var text = value ?? "";
            Publish(s => s.CalloutText == text ? s : s with { CalloutText = text });
        }
    }

    /// <summary>
    /// Show or hide a callout with further info next to the MyLocation symbol.
    /// </summary>
    public bool ShowCallout
    {
        get => _state.Latest.ShowCallout;
        set => Publish(s => s.ShowCallout == value ? s : s with { ShowCallout = value });
    }

    /// <summary>
    /// This event is triggered whenever the MyLocation symbol or label is clicked.
    /// </summary>
    public event EventHandler<MapEventArgs>? Tapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.Layers.MyLocationLayer"/> class.
    /// </summary>
    /// <param name="map">Map, to which this layer belongs</param>
    public MyLocationLayer(Map map) : this(map, new MPoint(0, 0))
    {
        _hasLocation = false; // (0, 0) is a placeholder, not a location to animate from.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.Layers.MyLocationLayer"/> class
    /// with a starting location.
    /// </summary>
    /// <param name="map">Map, to which this layer belongs</param>
    /// <param name="location">Location, where to start</param>
    public MyLocationLayer(Map map, MPoint location)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(location);
        location = new MPoint(location); // Copy so that a caller mutating its point later does not change the state.
        _hasLocation = true;

        _map = map;
        _map.Tapped += MapTapped;

        Enabled = true;

        // Each layer gets its own Image instances because Image is mutable (SvgFillColor etc.), so changing
        // one layer's image must not affect another layer.
        var initial = new State(
            Location: location,
            AnimateLocation: false,
            Direction: 0,
            DirectionViewportRotation: 0,
            AnimateDirection: false,
            ViewingDirection: -1,
            ViewingDirectionViewportRotation: 0,
            AnimateViewingDirection: false,
            IsMoving: false,
            IsCentered: true,
            StillImage: _stillImageSource,
            MovingImage: _movingImageSource,
            DirectionImage: _directionImageSource,
            Scale: 1.0,
            DirectionScale: 0.2,
            CalloutText: "",
            ShowCallout: false);
        _state = new StateHandover<State>(initial);

        // The feature gets its own point because the render pipeline moves it during animations.
        _feature = new PointFeature(new MPoint(location))
        {
            ["Label"] = "MyLocation",
        };

        _locStyle = new ImageStyle
        {
            Enabled = true,
            Image = initial.StillImage,
            SymbolScale = initial.Scale,
            SymbolRotation = 0,
            Offset = new Offset(0, 0),
            Opacity = 1,
        };

        _dirStyle = new ImageStyle
        {
            Enabled = false,
            Image = initial.DirectionImage,
            SymbolScale = initial.DirectionScale,
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
    /// <param name="animated">true if animated</param>
    public void UpdateMyLocation(MPoint newLocation, bool animated = false)
    {
        ArgumentNullException.ThrowIfNull(newLocation);
        var location = new MPoint(newLocation); // Copy so that a caller mutating its point later does not change the state.
        // A non-animated request for the current target still has an effect: it snaps a running animation to its end.
        Publish(s => s.Location.Equals(location) && s.AnimateLocation == animated
            ? s
            : s with { Location = location, AnimateLocation = animated });
    }

    /// <summary>
    /// Updates my movement direction
    /// </summary>
    /// <param name="newDirection">New direction</param>
    /// <param name="newViewportRotation">New viewport rotation</param>
    /// <param name="animated">true if animated</param>
    public void UpdateMyDirection(double newDirection, double newViewportRotation, bool animated = false)
    {
        Publish(s => s.Direction == newDirection && s.DirectionViewportRotation == newViewportRotation && s.AnimateDirection == animated
            ? s
            : s with { Direction = newDirection, DirectionViewportRotation = newViewportRotation, AnimateDirection = animated });
    }

    /// <summary>
    /// Updates my speed
    /// </summary>
    /// <param name="newSpeed">New speed</param>
    public void UpdateMySpeed(double newSpeed) => IsMoving = newSpeed > 0;

    /// <summary>
    /// Updates my view direction. A negative direction hides the direction image.
    /// </summary>
    /// <param name="newDirection">New direction</param>
    /// <param name="newViewportRotation">New viewport rotation</param>
    /// <param name="animated">true if animated</param>
    public void UpdateMyViewDirection(double newDirection, double newViewportRotation, bool animated = false)
    {
        Publish(s => s.ViewingDirection == newDirection && s.ViewingDirectionViewportRotation == newViewportRotation && s.AnimateViewingDirection == animated
            ? s
            : s with { ViewingDirection = newDirection, ViewingDirectionViewportRotation = newViewportRotation, AnimateViewingDirection = animated });
    }

    /// <summary>
    /// Applies the requested state and advances the animations. Called by the render pipeline once per frame.
    /// </summary>
    public override bool UpdateAnimations()
    {
        if (_state.TryTake(out var previous, out var latest))
            Apply(previous, latest);

        return _animations.Update(_map);
    }

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        return _features;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _map.Tapped -= MapTapped;
        base.Dispose(disposing);
    }

    /// <summary>
    /// Publishes a change to the requested state and wakes the render pipeline. Safe to call from any thread.
    /// </summary>
    private void Publish(Func<State, State> change)
    {
        var (previous, latest) = _state.Update(change);
        if (ReferenceEquals(previous, latest)) return;

        // The redraw area requested here is a best effort from the caller's thread: it does not know where
        // the feature is during an animation, and the viewport it reads may be mid-update. Apply requests the
        // authoritative area on the pipeline, in the same frame; the two are accumulated. What matters here
        // is that a request is made at all, because that is what wakes the pipeline.
        RequestRefresh(previous, latest, DirtyRectForSymbol(previous).Join(DirtyRectForSymbol(latest)));
    }

    private void RequestRefresh(State previous, State latest, MRect dirtyRect)
    {
        if (NeedsFullRefresh(previous, latest))
            _map.RefreshGraphics();
        else
            _map.RefreshGraphics(dirtyRect);
    }

    private bool NeedsFullRefresh(State previous, State latest)
    {
        // The callout size depends on the text, and a symbol that changes size leaves an unknown old extent.
        if (previous.CalloutText != latest.CalloutText || previous.ShowCallout != latest.ShowCallout) return true;
        if (previous.Scale != latest.Scale || previous.DirectionScale != latest.DirectionScale) return true;
        if (previous.StillImage != latest.StillImage || previous.MovingImage != latest.MovingImage) return true;
        if (previous.DirectionImage != latest.DirectionImage) return true;
        if (previous.ShowDirection != latest.ShowDirection) return true;
        // An image that was not drawn yet has an unknown size.
        if (previous.IsMoving != latest.IsMoving && !IsImageCached(latest.LocationImage)) return true;
        return false;
    }

    /// <summary>
    /// Applies the difference between two states to the styles and feature. Runs on the render pipeline.
    /// </summary>
    private void Apply(State previous, State latest)
    {
        // The symbol as it is drawn now: at the feature's current position, which during an animation is between
        // two locations, with the previous images and scales.
        var before = DirtyRectForSymbol(previous, _feature.Point);

        _locStyle.Image = latest.LocationImage;
        _locStyle.SymbolScale = latest.Scale;
        _dirStyle.Image = latest.DirectionImage;
        _dirStyle.SymbolScale = latest.DirectionScale;
        _dirStyle.Enabled = latest.ShowDirection;
        _coStyle.Title = latest.CalloutText;
        _coStyle.Enabled = latest.ShowCallout;

        // A non-animated request while an animation runs snaps the animation to its end.
        if (!previous.Location.Equals(latest.Location) || (!latest.AnimateLocation && _animationMyLocation is not null))
            ApplyLocation(latest);

        if (previous.LocationRotation != latest.LocationRotation || (!latest.AnimateDirection && _animationMyDirection is not null))
            ApplyRotation(_locStyle, latest.LocationRotation, latest.AnimateDirection, ref _animationMyDirection);

        if (!latest.ShowDirection)
            StopAnimation(ref _animationMyViewDirection);
        else if (previous.DirectionRotation != latest.DirectionRotation || !previous.ShowDirection || (!latest.AnimateViewingDirection && _animationMyViewDirection is not null))
            ApplyRotation(_dirStyle, latest.DirectionRotation, latest.AnimateViewingDirection && previous.ShowDirection, ref _animationMyViewDirection);

        // Redraw the symbol where it was and where it is now. This runs before the pipeline takes the pending
        // refresh, so it is part of the frame being drawn. Animation ticks request their own areas later.
        RequestRefresh(previous, latest, before.Join(DirtyRectForSymbol(latest, _feature.Point)));
    }

    private void ApplyLocation(State latest)
    {
        // A new location aborts the running animation. The final jumps to its end and redraws that path.
        StopAnimation(ref _animationMyLocation);

        var start = new MPoint(_feature.Point);
        var end = latest.Location;
        var animate = latest.AnimateLocation && _hasLocation && _map.Navigator.Viewport.ToExtent() is not null;
        _hasLocation = true;

        if (animate)
        {
            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;

            // Refresh the end viewport at the start of the animation so it has time to load.
            var endViewport = _map.Navigator.Viewport with { CenterX = end.X, CenterY = end.Y };
            _map.RefreshData(endViewport);

            _animationMyLocation = new AnimationEntry<Map>(
                start,
                end,
                animationStart: 0,
                animationEnd: 1,
                tick: (map, entry, v) =>
                {
                    var dirtyRect = MoveFeature(new MPoint(start.X + deltaX * v, start.Y + deltaY * v));
                    if (dirtyRect is not null) map.RefreshGraphics(dirtyRect);
                    return new AnimationResult<Map>(map, true);
                },
                final: (map, entry) =>
                {
                    var dirtyRect = MoveFeature(end);
                    if (dirtyRect is not null) map.RefreshGraphics(dirtyRect);
                    return new AnimationResult<Map>(map, false);
                });

            _animations.Start(_animationMyLocation, _animationDuration);

            if (latest.IsCentered)
                _map.Navigator.CenterOn(end, _animationDuration, Easing.Linear);
        }
        else
        {
            MoveFeature(end); // Apply requests the redraw of the old and new position.

            if (latest.IsCentered)
                _map.Navigator.CenterOn(end);
        }
    }

    private void ApplyRotation(ImageStyle style, int newRotation, bool animated, ref AnimationEntry<Map>? animation)
    {
        StopAnimation(ref animation);

        var oldRotation = (int)style.SymbolRotation;
        if (newRotation == oldRotation) return;

        // Rotate the short way round across north.
        if (newRotation < 90 && oldRotation > 270)
            newRotation += 360;
        else if (newRotation > 270 && oldRotation < 90)
            oldRotation += 360;

        var endRotation = newRotation % 360;

        if (!animated)
        {
            style.SymbolRotation = endRotation; // Apply requests the redraw.
            return;
        }

        var diffRotation = newRotation - oldRotation;

        animation = new AnimationEntry<Map>(
            oldRotation,
            newRotation,
            animationStart: 0,
            animationEnd: 1,
            tick: (map, entry, v) =>
            {
                var symbolRotation = (oldRotation + (int)(v * diffRotation)) % 360;
                if (symbolRotation != (int)style.SymbolRotation)
                {
                    style.SymbolRotation = symbolRotation;
                    map.RefreshGraphics(DirtyRectForSymbol(_state.Applied, _feature.Point));
                }

                return new AnimationResult<Map>(map, true);
            },
            final: (map, entry) =>
            {
                if ((int)style.SymbolRotation != endRotation)
                {
                    style.SymbolRotation = endRotation;
                    map.RefreshGraphics(DirtyRectForSymbol(_state.Applied, _feature.Point));
                }

                return new AnimationResult<Map>(map, false);
            });

        _animations.Start(animation, _animationDuration);
    }

    private void StopAnimation(ref AnimationEntry<Map>? animation)
    {
        _animations.Stop(_map, animation, callFinal: true);
        animation = null;
    }

    /// <summary>
    /// Moves the feature and returns the area that needs a redraw, or null when it did not move.
    /// </summary>
    private MRect? MoveFeature(MPoint newLocation)
    {
        if (_feature.Point.Equals(newLocation)) return null;
        var state = _state.Applied;
        var oldRect = DirtyRectForSymbol(state, _feature.Point);
        _feature.Point.X = newLocation.X;
        _feature.Point.Y = newLocation.Y;
        _feature.Modified();
        // Union old and new symbol extents so the old symbol position is also cleared.
        return oldRect.Join(DirtyRectForSymbol(state, newLocation));
    }

    private MRect DirtyRectForSymbol(State state) => DirtyRectForSymbol(state, state.Location);

    private MRect DirtyRectForSymbol(State state, MPoint location)
    {
        // Full symbol size used as the half-extent on each side — a conservative world-space bound
        // that covers the rendered pixel footprint at any rotation, regardless of sub-pixel offsets or anti-aliasing.
        var symbolSize = GetSymbolSize(state.LocationImage, state.Scale);
        if (state.ShowDirection)
            symbolSize = Math.Max(symbolSize, GetSymbolSize(state.DirectionImage, state.DirectionScale));
        var halfWorld = symbolSize * _map.Navigator.Viewport.Resolution;
        return new MRect(location.X - halfWorld, location.Y - halfWorld, location.X + halfWorld, location.Y + halfWorld);
    }

    private double GetSymbolSize(Image image, double scale)
    {
        // Custom images can be any size, so use the size of the image the renderer has already drawn.
        // This lookup never creates an image; rendering belongs in the renderer. An image that is not drawn
        // yet falls back to the default size, so a change that shows a new image requests a full refresh instead.
        var drawn = GetCachedImage(image);
        var size = drawn is null ? SymbolStyle.DefaultWidth : Math.Max(drawn.Width, drawn.Height);
        return size * scale;
    }

    private bool IsImageCached(Image image) => GetCachedImage(image) is not null;

    private IDrawableImage? GetCachedImage(Image image) => _map.RenderService.DrawableImageCache.GetIfCreated(image.SourceId);

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
