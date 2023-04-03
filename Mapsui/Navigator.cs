using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Limiting;
using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui;

public class Navigator
{
    private Viewport _viewport = new(0, 0, 1, 0, 0, 0);
    private IEnumerable<AnimationEntry<Viewport>> _animations = Enumerable.Empty<AnimationEntry<Viewport>>();

    /// <summary>
    /// Called when a data refresh is needed. This directly after a non-animated viewport change
    /// is made and after an animation has completed.
    /// </summary>
    public event EventHandler? RequestDataRefresh;
    public event PropertyChangedEventHandler? ViewportChanged;

    /// <summary>
    /// Sets the extent used to restrict panning. Exactly how this extent affects panning
    /// depends on the implementation of the IViewportLimiter.
    /// </summary>
    public MRect? PanExtent { get; set; }

    /// <summary>
    /// A pair of the most extreme resolutions (smallest and biggest). How these extremes affect zooming
    /// depends on the implementation of the IViewportLimiter.
    /// </summary>
    public MMinMax? ZoomExtremes { get; set; }

    public IViewportLimiter Limiter { get; set; } = new ViewportLimiter();

    public Viewport Viewport
    {
        get => _viewport;
        private set
        {
            if (_viewport == value) return;
            _viewport = value;
            OnViewportChanged();
        }
    }

    /// <summary>
    /// List of resolutions that can be used when going to a new zoom level. In the most common
    /// case these resolutions correspond to the resolutions of the background layer of the map. 
    /// In the Mapsui samples this is usually the openstreetmap layer, but there are also situations
    /// where this is no background layer with resolutions. Or where one app switches between different 
    /// background layers with different resolutions. Also note that when pinch zooming these resolutions 
    /// are not used.
    /// </summary>
    public IReadOnlyList<double> Resolutions { get; set; } = new List<double>();

    public MouseWheelAnimation MouseWheelAnimation { get; } = new();

    public void MouseWheelZoom(int mouseWheelDelta, MPoint centerOfZoom)
    {
        if (!Viewport.HasSize()) return;

        // It is unexpected that this method uses the MouseWheelAnimation.Animation and Easing. 
        // At the moment this solution allows the user to change these fields, so I don't want
        // them to become hardcoded values in the MapControl. There should be a more general
        // way to control the animation parameters.
        var resolution = MouseWheelAnimation.GetResolution(mouseWheelDelta, Viewport.Resolution, ZoomExtremes, Resolutions);
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
    /// Zooms the viewport to show the box. The boxFit parameter can be used to deal with a difference in 
    /// the width/height ratio between the viewport and the box. The center and resolution will change accordingly.
    /// </summary>
    /// <param name="box">The box to show in the viewport.</param>
    /// <param name="boxFit">The way the box should be fit into the view.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomToBox(MRect? box, MBoxFit boxFit = MBoxFit.Fit, long duration = -1, Easing? easing = default)
    {
        if (!Viewport.HasSize()) return;
        if (box == null) return;
        if (box.Width <= 0 || box.Height <= 0) return;

        var resolution = ZoomHelper.CalculateResolutionForWorldSize(
            box.Width, box.Height, Viewport.Width, Viewport.Height, boxFit);

        CenterOnAndZoomTo(box.Centroid, resolution, duration, easing);
    }

    /// <summary>
    /// Navigate to the Navigator.PanExtent.
    /// </summary>
    /// <param name="boxFit">Scale method to use to determine resolution</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomToPanExtent(MBoxFit boxFit = MBoxFit.Fill, long duration = -1, Easing? easing = default)
    {
        if (!Viewport.HasSize()) return;
        if (PanExtent is null)
        {
            Logger.Log(LogLevel.Warning, "ZoomToPanExtent was called but PanExtent was null");
            return;
        }
        
        ZoomToBox(PanExtent, boxFit, duration, easing);
    }

    /// <summary>
    /// Navigate to center and change resolution with animation
    /// </summary>
    /// <param name="center">New center to move to</param>
    /// <param name="resolution">New resolution to use</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void CenterOnAndZoomTo(MPoint center, double resolution, long duration = -1, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        if (Limiter.ZoomLock) return;

        var newViewport = Viewport with { CenterX = center.X, CenterY = center.Y, Resolution = resolution };
        SetViewport(newViewport, duration, easing);
    }

    /// <summary>
    /// Change resolution of the viewport
    /// </summary>
    /// <param name="resolution">New resolution to use</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomTo(double resolution, long duration = -1, Easing? easing = default)
    {
        if (Limiter.ZoomLock) return;

        var newViewport = Viewport with { Resolution = resolution };
        SetViewport(newViewport, duration, easing);
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
    public void ZoomTo(double resolution, MPoint centerOfZoomInScreenCoordinates, long duration = -1, Easing? easing = default)
    {
        if (!Viewport.HasSize()) return;
        if (Limiter.ZoomLock) return;

        var (centerOfZoomX, centerOfZoomY) = Viewport.ScreenToWorldXY(centerOfZoomInScreenCoordinates.X, centerOfZoomInScreenCoordinates.Y);

        if (Limiter.PanLock)
        {
            // Avoid pan by zooming on center
            centerOfZoomX = Viewport.CenterX;
            centerOfZoomY = Viewport.CenterY;
        }

        var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, resolution, Viewport.CenterX, Viewport.CenterY, Viewport.Resolution);
        var newViewport = Viewport with { CenterX = x, CenterY = y, Resolution = resolution };

        SetViewport(newViewport, duration, easing);
    }

    /// <summary>
    /// Zoom in to the next resolutionin in the Navigator.Resolutions list.
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomIn(long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.GetResolutionToZoomIn(Resolutions, Viewport.Resolution);
        ZoomTo(resolution, duration, easing);
    }

    /// <summary>
    /// Zoom out to the next resolution in the Navigator.Resolutions list.
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomOut(long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.GetResolutionToZoomOut(Resolutions, Viewport.Resolution);
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
        var resolution = ZoomHelper.GetResolutionToZoomIn(Resolutions, Viewport.Resolution);
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
        var resolution = ZoomHelper.GetResolutionToZoomOut(Resolutions, Viewport.Resolution);
        ZoomTo(resolution, centerOfZoom, duration, easing);
    }

    /// <summary>
    /// Zooms to the level indicated. The level is the index of the resolution in the Navigator.Resolutions list.
    /// </summary>
    /// <param name="level">The index of the Navigator.Resolutions list.</param>
    public void ZoomToLevel(int level)
    {
        if (level < 0 || level >= Resolutions.Count)
        {
            Logger.Log(LogLevel.Warning, $"Zoom level '{level}' is not an index in the range of the resolutions list");
            return;
        }
        ZoomTo(Resolutions[level]);
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
    public void CenterOn(MPoint center, long duration = -1, Easing? easing = default)
    {
        if (Limiter.PanLock) return;

        var newViewport = Viewport with { CenterX = center.X, CenterY = center.Y };
        SetViewport(newViewport, duration, easing);
    }

    /// <summary>
    /// Fly to the given center with zooming out to given resolution and in again
    /// </summary>
    /// <param name="center">MPoint to fly to</param>
    /// <param name="maxResolution">Maximum resolution to zoom out</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    public void FlyTo(MPoint center, double maxResolution, long duration = 500)
    {
        _animations = FlyToAnimation.Create(Viewport, center, maxResolution, duration);
    }

    /// <summary>
    /// Change rotation of the viewport
    /// </summary>
    /// <param name="rotation">New rotation in degrees of the viewport></param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void RotateTo(double rotation, long duration = -1, Easing? easing = default)
    {
        if (Limiter.RotationLock) return;

        var newViewport = Viewport with { Rotation = rotation };
        SetViewport(newViewport, duration, easing);
    }

    /// <summary>
    /// Animate Fling of the viewport. This method is called from
    /// the MapControl and is usually not called from user code. This method does not call
    /// Navigated. 
    /// </summary>
    /// <param name="velocityX">VelocityX from SwipedEventArgs></param>
    /// <param name="velocityY">VelocityX from SwipedEventArgs></param>
    /// <param name="maxDuration">Maximum duration of fling deceleration></param>
    public void Fling(double velocityX, double velocityY, long maxDuration)
    {
        if (Limiter.PanLock) return;

        _animations = FlingAnimation.Create(velocityX, velocityY, maxDuration);
    }

    /// <summary>
    /// To pan the map when dragging with mouse or single finger. This method is called from
    /// the MapControl and is usually not called from user code. This method does not call
    /// Navigated. So, Navigated needs to be called from the MapControl on mouse/touch up.
    /// </summary>
    /// <param name="positionScreen">Screen position of the dragging mouse or finger.</param>
    /// <param name="previousPositionScreen">Previous position of the dragging mouse or finger.</param>
    public void Drag(MPoint positionScreen, MPoint previousPositionScreen)
    {
        Pinch(positionScreen, previousPositionScreen, 1);
    }

    /// <summary>
    /// To change the map viewport when using multiple fingers. This method is called from
    /// the MapControl and is usually not called from user code. This method does not call
    /// Navigated. So, Navigated needs to be called from the MapControl on mouse/touch up.
    /// </summary>
    /// <param name="currentPinchCenter">The center of the current position of touch positions.</param>
    /// <param name="previousPinchCenter">The previous center of the current position of touch positions.</param>
    /// <param name="deltaResolution">The change in resolution cause by moving the fingers together or further apart.</param>
    /// <param name="deltaRotation">The change in rotation of the finger positions.</param>
    public void Pinch(MPoint currentPinchCenter, MPoint previousPinchCenter, double deltaResolution, double deltaRotation = 0)
    {
        if (Limiter.ZoomLock) deltaResolution = 1;
        if (Limiter.PanLock) currentPinchCenter = previousPinchCenter;

        ClearAnimations();

        var viewport = TransformState(Viewport, currentPinchCenter, previousPinchCenter, deltaResolution, deltaRotation);
        SetViewportWithLimit(viewport);
    }

    public void SetSize(double width, double height)
    {
        ClearAnimations();
        SetViewportWithLimit(Viewport with { Width = width, Height = height });
        OnRequestDataRefresh();

    }

    private void OnRequestDataRefresh()
    {
        RequestDataRefresh?.Invoke(this, EventArgs.Empty);
    }

    private static Viewport TransformState(Viewport viewport, MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution, double deltaRotation)
    {
        var previous = viewport.ScreenToWorld(previousPositionScreen.X, previousPositionScreen.Y);
        var current = viewport.ScreenToWorld(positionScreen.X, positionScreen.Y);

        var newX = viewport.CenterX + previous.X - current.X;
        var newY = viewport.CenterY + previous.Y - current.Y;

        if (deltaResolution == 1 && deltaRotation == 0 && viewport.CenterX == newX && viewport.CenterY == newY)
            return viewport;

        if (deltaResolution != 1)
        {
            viewport = viewport with { Resolution = viewport.Resolution / deltaResolution };

            // Calculate current position again with adjusted resolution
            // Zooming should be centered on the place where the map is touched.
            // This is done with the scale correction.
            var scaleCorrectionX = (1 - deltaResolution) * (current.X - viewport.CenterX);
            var scaleCorrectionY = (1 - deltaResolution) * (current.Y - viewport.CenterY);

            newX -= scaleCorrectionX;
            newY -= scaleCorrectionY;
        }

        viewport = viewport with { CenterX = newX, CenterY = newY };

        if (deltaRotation != 0)
        {
            current = viewport.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            viewport = viewport with { Rotation = viewport.Rotation + deltaRotation };
            var postRotation = viewport.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            viewport = viewport with { CenterX = viewport.CenterX - (postRotation.X - current.X), CenterY = viewport.CenterY - (postRotation.Y - current.Y) };
        }

        return viewport;
    }

    private void ClearAnimations()
    {
        if (_animations.Any())
        {
            _animations = Enumerable.Empty<AnimationEntry<Viewport>>();
        }
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
        {
            ClearAnimations();
            OnRequestDataRefresh();
        }
        var animationResult = Animation.UpdateAnimations(Viewport, _animations);

        SetViewportWithLimit(animationResult.State);

        if (ShouldAnimationsBeHaltedBecauseOfLimiting(animationResult.State, Viewport))
        {
            ClearAnimations();
            OnRequestDataRefresh();
            return false; // Not running
        }

        return animationResult.IsRunning;
    }

    public void SetViewportAnimations(List<AnimationEntry<Viewport>> animations)
    {
        _animations = animations;
    }

    private void SetViewportWithLimit(Viewport viewport)
    {
        Viewport = Limit(viewport);
    }

    private bool ShouldAnimationsBeHaltedBecauseOfLimiting(Viewport input, Viewport output)
    {
        var zoomLimited = input.Resolution != output.Resolution;
        var fullyLimited =
            input.CenterX != output.CenterX &&
            input.CenterY != output.CenterY &&
            zoomLimited;

        // When the viewport is limited in x, y and resolution there will be no 
        // further change in subsequent updates and the animation should be halted.
        if (fullyLimited)
            return true;

        // When the animation hits the zoom limit it should also be halted. 
        // A further animation in the x or y direction appears as a confusing
        // drift of the viewport.
        return zoomLimited;
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

    public void SetViewport(Viewport viewport, long duration = -1, Easing? easing = default)
    {
        if (duration <= 0)
        {
            ClearAnimations();
            SetViewportWithLimit(viewport);
            OnRequestDataRefresh();
        }
        else
        {
            if (_animations.Any())
                OnRequestDataRefresh();
            _animations = ViewportAnimation.Create(Viewport, viewport, duration, easing);
        }
    }

    internal int GetAnimationsCount => _animations.Count();
}
