using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Limiting;
using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui;

public class Navigator : INavigator
{
    private Viewport _viewport = new(0, 0, 1, 0, 0, 0);
    private IEnumerable<AnimationEntry<Viewport>> _animations = Enumerable.Empty<AnimationEntry<Viewport>>();

    public EventHandler<ChangeType>? Navigated { get; set; }

    public event PropertyChangedEventHandler? ViewportChanged;

    /// <inheritdoc />
    public MRect? PanExtent { get; set; }

    /// <inheritdoc />
    public MMinMax? ZoomExtremes { get; set; }

    public IViewportLimiter Limiter { get; set; } = new ViewportLimiter();

    public Viewport State
    {
        get => _viewport;
        private set
        {
            if (_viewport == value) return;
            _viewport = value;
            OnViewportChanged();
        }
    }


    /// <inheritdoc />
    public IReadOnlyList<double> Resolutions { get; set; } = new List<double>();

    public MouseWheelAnimation MouseWheelAnimation { get; } = new();

    public void ZoomInOrOut(int mouseWheelDelta, MPoint centerOfZoom)
    {
        // Todo: Find a way in which the Navigator argument is not needed.
        var resolution = MouseWheelAnimation.GetResolution(mouseWheelDelta, this, Resolutions);
        if (mouseWheelDelta > Constants.Epsilon)
        {
            ZoomTo(resolution, centerOfZoom, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
        }
        else if (mouseWheelDelta < Constants.Epsilon)
        {
            ZoomTo(resolution, centerOfZoom, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
        }
    }

    /// <summary>
    /// Navigate center of viewport to center of extent and change resolution
    /// </summary>
    /// <param name="extent">New extent for viewport to show</param>
    /// <param name="scaleMethod">Scale method to use to determine resolution</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void NavigateTo(MRect? extent, ScaleMethod scaleMethod = ScaleMethod.Fit, long duration = -1, Easing? easing = default)
    {
        if (extent == null) return;

        var resolution = ZoomHelper.DetermineResolution(
            extent.Width, extent.Height, State.Width, State.Height, scaleMethod);

        NavigateTo(extent.Centroid, resolution, duration, easing);
    }

    /// <summary>
    /// Navigate to a resolution, so such the map uses the fill method
    /// </summary>
    /// <param name="scaleMethod">Scale method to use to determine resolution</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomToPanExtent(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = -1, Easing? easing = default)
    {
        if (PanExtent is not null)
            NavigateTo(PanExtent, scaleMethod, duration, easing);
        else
            Logger.Log(LogLevel.Warning, "ZoomToPanExtent was called but PanExtent was null");
    }

    /// <summary>
    /// Navigate to center and change resolution with animation
    /// </summary>
    /// <param name="center">New center to move to</param>
    /// <param name="resolution">New resolution to use</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void NavigateTo(MPoint center, double resolution, long duration = 0, Easing? easing = default)
    {
        if (center == null) throw new ArgumentNullException(nameof(center));

        SetCenterAndResolution(center.X, center.Y, resolution, duration, easing);
        OnNavigated(duration, ChangeType.Discrete);

    }

    /// <summary>
    /// Change resolution of the viewport
    /// </summary>
    /// <param name="resolution">New resolution to use</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomTo(double resolution, long duration = 0, Easing? easing = default)
    {
        SetResolution(resolution, duration, easing);
        OnNavigated(duration, ChangeType.Discrete);
    }

    /// <summary>
    /// Zoom to a given resolution with a given point as center
    /// </summary>
    /// <param name="resolution">Resolution to zoom</param>
    /// <param name="centerOfZoomInScreenCoordinates">Center of zoom in screen coordinates. This is the one point in the map that 
    /// stays on the same location while zooming in. 
    /// For instance, in mouse wheel zoom animation the position 
    /// of the mouse pointer can be the center of zoom. Note, that the centerOfZoom is in screen coordinates not 
    /// world coordinates, this is because this is most convenient for the main use case, zoom with the mouse 
    /// position as center.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The easing of the animation when duration is > 0</param>
    public void ZoomTo(double resolution, MPoint centerOfZoomInScreenCoordinates, long duration = 0, Easing? easing = default)
    {
        if (Limiter.ZoomLock) return;

        var (centerOfZoomX, centerOfZoomY) = State.ScreenToWorldXY(centerOfZoomInScreenCoordinates.X, centerOfZoomInScreenCoordinates.Y);

        if (Limiter.PanLock)
        {
            // Avoid pan by zooming on center
            centerOfZoomX = State.CenterX;
            centerOfZoomY = State.CenterY;
        }

        if (duration == 0)
        {
            // Todo: If there is limiting of one dimension the other dimension should be limited accordingly. 
            var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
                centerOfZoomX, centerOfZoomY, resolution, State.CenterX, State.CenterY, State.Resolution);
            SetViewportWithLimit(State with { CenterX = x, CenterY = y, Resolution = resolution });
            OnNavigated(ChangeType.Discrete);
        }
        else
        {
            var animationEntries = ZoomAroundLocationAnimation.Create(State, centerOfZoomX, centerOfZoomY, resolution,
                State, duration, easing ?? Easing.SinInOut);
            AddFinalAction(animationEntries, () => OnNavigated(ChangeType.Discrete));
            SetViewportAnimations(animationEntries);
        }

    }

    /// <summary>
    /// Zoom in to the next resolution
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomIn(long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.ZoomIn(Resolutions, State.Resolution);

        ZoomTo(resolution, duration, easing);
    }

    /// <summary>
    /// Zoom out to the next resolution
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomOut(long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.ZoomOut(Resolutions, State.Resolution);

        ZoomTo(resolution, duration, easing);
    }

    /// <summary>
    /// Zoom in to a given point
    /// </summary>
    /// <param name="centerOfZoom">Center of zoom. This is the one point in the map that stays on the same location while zooming in.
    /// For instance, in mouse wheel zoom animation the position of the mouse pointer can be the center of zoom.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomIn(MPoint centerOfZoom, long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.ZoomIn(Resolutions, State.Resolution);

        ZoomTo(resolution, centerOfZoom, duration, easing);
    }

    /// <summary>
    /// Zoom out to a given point
    /// </summary>
    /// <param name="centerOfZoom">Center of zoom. This is the one point in the map that stays on the same location while zooming in.
    /// For instance, in mouse wheel zoom animation the position of the mouse pointer can be the center of zoom.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomOut(MPoint centerOfZoom, long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.ZoomOut(Resolutions, State.Resolution);
        ZoomTo(resolution, centerOfZoom, duration, easing);
    }

    /// <summary>
    /// Change center of the viewport to X/Y coordinates
    /// </summary>
    /// <param name="x">X value of the new center</param>
    /// <param name="y">Y value of the new center</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">Function for easing</param>
    public void CenterOn(double x, double y, long duration = -1, Easing? easing = default)
    {
        CenterOn(new MPoint(x, y), duration, easing);
    }

    /// <summary>
    /// Change center of the viewport
    /// </summary>
    /// <param name="center">New center point of the viewport</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">Function for easing</param>
    public void CenterOn(MPoint center, long duration = 0, Easing? easing = default)
    {
        SetCenter(center, duration, easing);
        OnNavigated(duration, ChangeType.Discrete);
    }

    /// <summary>
    /// Fly to the given center with zooming out to given resolution and in again
    /// </summary>
    /// <param name="center">MPoint to fly to</param>
    /// <param name="maxResolution">Maximum resolution to zoom out</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    public void FlyTo(MPoint center, double maxResolution, long duration = 500)
    {
        var animationEntries = FlyToAnimation.Create(State, center, maxResolution, duration);
        AddFinalAction(animationEntries, () => OnNavigated(ChangeType.Discrete));
        SetViewportAnimations(animationEntries);
    }

    /// <summary>
    /// Change rotation of the viewport
    /// </summary>
    /// <param name="rotation">New rotation in degrees of the viewport></param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void RotateTo(double rotation, long duration = 0, Easing? easing = default)
    {
        SetRotation(rotation, duration, easing);

        OnNavigated(duration, ChangeType.Discrete);
    }

    /// <summary>
    /// Animate Fling of the viewport
    /// </summary>
    /// <param name="velocityX">VelocityX from SwipedEventArgs></param>
    /// <param name="velocityY">VelocityX from SwipedEventArgs></param>
    /// <param name="maxDuration">Maximum duration of fling deceleration></param>
    public void FlingWith(double velocityX, double velocityY, long maxDuration)
    {
        if (Limiter.PanLock) return;

        var response = FlingAnimation.Create(velocityX, velocityY, maxDuration);
        SetViewportAnimations(response.Entries);
        OnNavigated(response.Duration, ChangeType.Discrete);
    }

    /// <summary> Adds the final action. </summary>
    /// <param name="animationEntries">The animation entries.</param>
    /// <param name="action">The action.</param>
    private void AddFinalAction(List<AnimationEntry<Viewport>> animationEntries, Action action)
    {
        var entry = animationEntries.FirstOrDefault();
        if (entry != null)
        {
            animationEntries.Add(new AnimationEntry<Viewport>(entry.Start, entry.End, final: (v, a) => { action(); return new AnimationResult<Viewport>(v, true); }));
        }
    }

    private void OnNavigated(long duration, ChangeType changeType)
    {
        // Note. Instead of a delay it may also be possible to call Navigated immediately with the viewport state
        // that is the result of the animation.
        _ = Task.Delay((int)duration).ContinueWith(t => OnNavigated(changeType), TaskScheduler.Default);
    }

    private void OnNavigated(ChangeType changeType)
    {
        Navigated?.Invoke(this, changeType);
    }

  
    /// <inheritdoc />
    public void Transform(MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution = 1, double deltaRotation = 0)
    {
        if (Limiter.ZoomLock) deltaResolution = 1;
        if (Limiter.PanLock) positionScreen = previousPositionScreen;

        ClearAnimations();

        State = Limit(TransformState(_viewport, positionScreen, previousPositionScreen, deltaResolution, deltaRotation));
    }

    private static Viewport TransformState(Viewport state, MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution, double deltaRotation)
    {
        var previous = state.ScreenToWorld(previousPositionScreen.X, previousPositionScreen.Y);
        var current = state.ScreenToWorld(positionScreen.X, positionScreen.Y);

        var newX = state.CenterX + previous.X - current.X;
        var newY = state.CenterY + previous.Y - current.Y;

        if (deltaResolution == 1 && deltaRotation == 0 && state.CenterX == newX && state.CenterY == newY)
            return state;

        if (deltaResolution != 1)
        {
            state = state with { Resolution = state.Resolution / deltaResolution };

            // Calculate current position again with adjusted resolution
            // Zooming should be centered on the place where the map is touched.
            // This is done with the scale correction.
            var scaleCorrectionX = (1 - deltaResolution) * (current.X - state.CenterX);
            var scaleCorrectionY = (1 - deltaResolution) * (current.Y - state.CenterY);

            newX -= scaleCorrectionX;
            newY -= scaleCorrectionY;
        }

        state = state with { CenterX = newX, CenterY = newY };

        if (deltaRotation != 0)
        {
            current = state.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            state = state with { Rotation = state.Rotation + deltaRotation };
            var postRotation = state.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            state = state with { CenterX = state.CenterX - (postRotation.X - current.X), CenterY = state.CenterY - (postRotation.Y - current.Y) };
        }

        return state;
    }

    public void SetSize(double width, double height)
    {
        ClearAnimations();


        var newState = _viewport with { Width = width, Height = height };
        newState = Limit(newState);
        State = newState;
    }

    // Todo: Make private or merge with caller
    public void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
    {
        // Todo: Fix the unused animation parameters.

        if (Limiter.PanLock) return;
        ClearAnimations();

        var newState = Limit(_viewport with { CenterX = x, CenterY = y });
        State = newState;
    }

    // Todo: Make private or merge with caller
    public void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        if (Limiter.ZoomLock) return;

        ClearAnimations();

        var newState = _viewport with { CenterX = x, CenterY = y, Resolution = resolution };
        newState = Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(State, newState, duration, easing);
    }

    // Todo: Make private or merge with caller
    public void SetCenter(MPoint center, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;

        ClearAnimations();

        var newState = _viewport with { CenterX = center.X, CenterY = center.Y };
        newState = Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(State, newState, duration, easing);
    }

    // Todo: Make private or merge with caller
    public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
    {
        if (Limiter.ZoomLock) return;

        ClearAnimations();

        var newState = _viewport with { Resolution = resolution };
        newState = Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(State, newState, duration, easing);
    }

    private void ClearAnimations()
    {
        _animations = Enumerable.Empty<AnimationEntry<Viewport>>();
    }

    // Todo: Merge with caller
    private void SetRotation(double rotation, long duration = 0, Easing? easing = default)
    {
        if (Limiter.RotationLock) return;

        ClearAnimations();

        var newState = _viewport with { Rotation = rotation };
        newState = Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(State, newState, duration, easing);
    }

    /// <summary>
    /// Property change event
    /// </summary>
    /// <param name="propertyName">Name of property that changed</param>
    private void OnViewportChanged([CallerMemberName] string? propertyName = null)
    {
        ViewportChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool UpdateAnimations()
    {
        if (!_animations.Any()) return false;
        if (_animations.All(a => a.Done))
            ClearAnimations();

        var result = Animation.UpdateAnimations(State, _animations);


        var limitResult = SetViewportWithLimit(result.CurrentState);
        if (limitResult.ZoomLimited || limitResult.FullyLimited)
            ClearAnimations();

        return result.IsRunning;
    }

    public void SetViewportAnimations(List<AnimationEntry<Viewport>> animations)
    {
        _animations = animations;
    }

    public LimitResult SetViewportWithLimit(Viewport viewport)
    {
        State = Limit(viewport);
        return new LimitResult(viewport, State);
    }

    /// <summary>
    /// To make the other limiting call in this class a bit shorter.
    /// </summary>
    /// <param name="viewport"></param>
    /// <returns></returns>
    private Viewport Limit(Viewport viewport)
    {
        return Limiter.Limit(viewport, PanExtent, ZoomExtremes);
    }
}
