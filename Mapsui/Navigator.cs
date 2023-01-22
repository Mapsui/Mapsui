using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Utilities;
using Mapsui.ViewportAnimations;

namespace Mapsui;

public class Navigator : INavigator
{
    private readonly Map _map;
    private readonly IViewport _viewport;

    public EventHandler<ChangeType>? Navigated { get; set; }

    public Navigator(Map map, IViewport viewport)
    {
        _map = map;
        _viewport = viewport;
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
            extent.Width, extent.Height, _viewport.Width, _viewport.Height, scaleMethod);

        NavigateTo(extent.Centroid, resolution, duration, easing);
    }

    /// <summary>
    /// Navigate to a resolution, so such the map uses the fill method
    /// </summary>
    /// <param name="scaleMethod">Scale method to use to determine resolution</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = -1, Easing? easing = default)
    {
        if (_map.Extent != null)
            NavigateTo(_map.Extent, scaleMethod, duration, easing);
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

        _viewport.SetCenterAndResolution(center.X, center.Y, resolution, duration, easing);
        OnNavigated(duration, ChangeType.Discrete);

    }

    /// <summary>
    /// Change resolution of viewport
    /// </summary>
    /// <param name="resolution">New resolution to use</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomTo(double resolution, long duration = 0, Easing? easing = default)
    {
        _viewport.SetResolution(resolution, duration, easing);
        OnNavigated(duration, ChangeType.Discrete);
    }

    /// <summary>
    /// Zoom to a given resolution with a given point as center
    /// </summary>
    /// <param name="resolution">Resolution to zoom</param>
    /// <param name="centerOfZoom">Center of zoom in screen coordinates. This is the one point in the map that 
    /// stays on the same location while zooming in. /// For instance, in mouse wheel zoom animation the position 
    /// of the mouse pointer can be the center of zoom. Note, that the centerOfZoom is in screen coordinates not 
    /// world coordinates, this is because this is most convenient for the main use case, zoom with the mouse 
    /// position as center.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The easing of the animation when duration is > 0</param>
    public void ZoomTo(double resolution, MPoint centerOfZoom, long duration = 0, Easing? easing = default)
    {
        var (worldCenterOfZoomX, worldCenterOfZoomY) = _viewport.ScreenToWorldXY(centerOfZoom.X, centerOfZoom.Y);
        var animationEntries = ZoomAroundLocationAnimation.Create(_viewport, worldCenterOfZoomX, worldCenterOfZoomY, resolution,
            _viewport.CenterX, _viewport.CenterY, _viewport.Resolution, duration);
        AddFinalAction(animationEntries, () => OnNavigated(ChangeType.Discrete));
        _viewport.SetAnimations(animationEntries);

    }

    /// <summary>
    /// Zoom in to the next resolution
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomIn(long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

        ZoomTo(resolution, duration, easing);
    }

    /// <summary>
    /// Zoom out to the next resolution
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomOut(long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);

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
        var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

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
        var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
        ZoomTo(resolution, centerOfZoom, duration, easing);
    }

    /// <summary>
    /// Change center of viewport to X/Y coordinates
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
    /// Change center of viewport
    /// </summary>
    /// <param name="center">New center point of viewport</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">Function for easing</param>
    public void CenterOn(MPoint center, long duration = 0, Easing? easing = default)
    {
        _viewport.SetCenter(center, duration, easing);
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
        var animationEntries = FlyToAnimation.Create(_viewport, center, maxResolution, duration);
        AddFinalAction(animationEntries, () => OnNavigated(ChangeType.Discrete));
        _viewport.SetAnimations(animationEntries);
    }

    /// <summary>
    /// Change rotation of viewport
    /// </summary>
    /// <param name="rotation">New rotation in degrees of viewport></param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void RotateTo(double rotation, long duration = 0, Easing? easing = default)
    {
        _viewport.SetRotation(rotation, duration, easing);

        OnNavigated(duration, ChangeType.Discrete);
    }

    /// <summary>
    /// Animate Fling of viewport
    /// </summary>
    /// <param name="velocityX">VelocityX from SwipedEventArgs></param>
    /// <param name="velocityY">VelocityX from SwipedEventArgs></param>
    /// <param name="maxDuration">Maximum duration of fling deceleration></param>
    public void FlingWith(double velocityX, double velocityY, long maxDuration)
    {
        var response = FlingAnimation.Create(velocityX, velocityY, maxDuration);
        _viewport.SetAnimations(response.Entries);
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
            animationEntries.Add(new AnimationEntry<Viewport>(entry.Start, entry.End, final: (v, a) => action()));
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
}
