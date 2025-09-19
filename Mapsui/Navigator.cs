﻿using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Limiting;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Mapsui;

public class Navigator
{
    private Viewport _viewport = new();
    private IEnumerable<AnimationEntry<Viewport>> _animations = [];
    private readonly List<Action> _postponedCalls;
    private MMinMax? _defaultZoomBounds;
    private MRect? _defaultPanBounds;
    private MMinMax? _overrideZoomBounds;
    private MRect? _overridePanBounds;
    private readonly object _postponedCallsLock = new();
    private bool _suppressNotifications = true;

    public delegate void ViewportChangedEventHandler(object sender, ViewportChangedEventArgs e);

    /// <summary>
    /// Called when a data refresh is needed. This directly after a non-animated viewport change
    /// is made and after an animation has completed.
    /// </summary>
    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;
    public event ViewportChangedEventHandler? ViewportChanged;

    /// <summary>
    /// When true the user can not pan (move) the map.
    /// </summary>
    public bool PanLock { get; set; }

    /// <summary>
    /// When true the user can not zoom the map
    /// </summary>
    public bool ZoomLock { get; set; }

    /// <summary>
    /// When true the user can not rotate the map
    /// </summary>
    public bool RotationLock { get; set; }

    /// <summary>
    /// The bounds to restrict panning. Exactly how these bounds affects panning
    /// depends on the implementation of the IViewportLimiter.
    /// </summary>
    public MRect? PanBounds => OverridePanBounds ?? DefaultPanBounds;

    /// <summary>
    /// The bounds of zooming, i.e. the smallest and biggest resolutions. 
    /// How these bounds affect zooming depends on the implementation of the 
    /// IViewportLimiter.
    /// </summary>
    public MMinMax? ZoomBounds => OverrideZoomBounds ?? DefaultZoomBounds;

    /// <summary>
    /// Overrides the default zoom bounds which are derived from the Map resolutions.
    /// </summary>

    /// <summary>
    /// After how many degrees start rotation to take place
    /// </summary>
    public double UnSnapRotation { get; set; } = 30;

    /// <summary>
    /// With how many degrees from 0 should map snap to 0 degrees
    /// </summary>
    public double ReSnapRotation { get; set; } = 5;

    public MMinMax? OverrideZoomBounds
    {
        get => _overrideZoomBounds;
        set
        {
            _overrideZoomBounds = value;
            InitializeIfNeeded();
        }
    }

    /// <summary>
    /// Overrides the default pan bounds which come from the Map extent.
    /// </summary>
    public MRect? OverridePanBounds
    {
        get => _overridePanBounds;
        set
        {
            _overridePanBounds = value;
            InitializeIfNeeded();
        }
    }

    /// <summary>
    /// Overrides the default resolutions which are derived from the Map.Layers resolutions.
    /// </summary>
    public IReadOnlyList<double>? OverrideResolutions { get; set; }

    public IViewportLimiter Limiter { get; set; } = new ViewportLimiter();

    public Viewport Viewport
    {
        get => _viewport;
        private set
        {
            if (_viewport == value) return;
            var oldViewport = _viewport;
            _viewport = value;
            OnViewportChanged(oldViewport, _viewport);
        }
    }

    public bool HasExecutedPostponedCalls { get; private set; } = false;

    public Navigator()
    {
        // Add a default postponed call which will be executed as soon as the size is known.
        _postponedCalls = new List<Action> { DefaultPostponedCall };
    }

    private void DefaultPostponedCall()
    {
        if (PanBounds is null)
        {
            Logger.Log(LogLevel.Information, $"Navigator: Not zooming to {nameof(PanBounds)} at startup because they are not set.");
        }
        else
        {
            Logger.Log(LogLevel.Information, $"Navigator: Zooming to {nameof(PanBounds)} at startup.");
            ZoomToPanBounds();
        }
    }

    private void ExecutedPostponedCalls()
    {
        lock (_postponedCallsLock)
        {
            if (!HasExecutedPostponedCalls)
            {
                HasExecutedPostponedCalls = true;

                // Actions could either modify the current state (after ZoomToBox above) or override the current state.
                foreach (var postponedCall in _postponedCalls)
                {
                    postponedCall();
                }

                _postponedCalls.Clear();

                _suppressNotifications = false; // Multiple postponed calls will trigger only one refresh because of suppression during startup.
                OnViewportChanged(_viewport, _viewport);
                OnFetchRequested(ChangeType.Discrete);
            }
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
    public IReadOnlyList<double> Resolutions => OverrideResolutions ?? DefaultResolutions;

    public MouseWheelAnimation MouseWheelAnimation { get; } = new();

    public void MouseWheelZoomContinuous(double scaleFactor, ScreenPosition centerOfZoom)
    {
        if (!Viewport.HasSize())
            return;
        if (ZoomLock)
            return;
        if (scaleFactor == 1)
            return; // No change
        if (scaleFactor <= Constants.Epsilon)
        {
            Logger.Log(LogLevel.Warning, "Navigator: MouseWheelZoomContinuous was called with a mouseWheelDelta <= 0. This is unexpected.");
            return;
        }

        if (PanLock) // Avoid pan by zooming on center
            centerOfZoom = Viewport.WorldToScreen(Viewport.CenterX, Viewport.CenterY);

        // MouseWheelAnimation tracks the destination resolution which allows for faster zooming in if the next tick
        // starts before the previous animation is finished. We may want something like that here as well.

        var resolution = Viewport.Resolution * scaleFactor;
        ZoomTo(resolution, centerOfZoom); // Not using animations for continuous zooming because the steps will be  small.
    }

    public void MouseWheelZoom(int mouseWheelDelta, ScreenPosition centerOfZoom)
    {
        if (ZoomLock)
            return;

        if (PanLock) // Avoid pan by zooming on center
            centerOfZoom = Viewport.WorldToScreen(Viewport.CenterX, Viewport.CenterY);

        // It is unexpected that this method uses the MouseWheelAnimation.Animation and Easing. 
        // At the moment this solution allows the user to change these fields, so I don't want
        // them to become hardcoded values in the MapControl. There should be a more general
        // way to control the animation parameters.
        var resolution = MouseWheelAnimation.GetResolution(
            mouseWheelDelta, Viewport.Resolution, ZoomBounds, Resolutions);
        if (resolution == Viewport.Resolution) return; // Do not start an animation when at the goal resolution.
        ZoomTo(resolution, centerOfZoom, MouseWheelAnimation.Duration, MouseWheelAnimation.Easing);
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
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => ZoomToBox(box, boxFit, duration, easing));
            return;
        }

        if (box == null)
        {
            if (Viewport.Resolution <= 0)
                Logger.Log(LogLevel.Warning, $"Navigator: {nameof(ZoomToBox)} was called but the {nameof(box)} was null. This is unexpected.");
            return;
        }
        if (box.Width <= 0 || box.Height <= 0) return;

        var resolution = ZoomHelper.CalculateResolutionForWorldSize(
            box.Width, box.Height, Viewport.Width, Viewport.Height, boxFit);

        CenterOnAndZoomTo(box.Centroid, resolution, duration, easing);
    }

    /// <summary>
    /// Navigate to the <see cref="PanBounds" />.
    /// </summary>
    /// <param name="boxFit">Scale method to use to determine resolution</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomToPanBounds(MBoxFit boxFit = MBoxFit.Fill, long duration = -1, Easing? easing = default)
    {
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => ZoomToPanBounds(boxFit, duration, easing));
            return;
        }

        ZoomToBox(PanBounds, boxFit, duration, easing);
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
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => CenterOnAndZoomTo(center, resolution, duration, easing));
            return;
        }

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
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => ZoomTo(resolution, duration, easing));
            return;
        }

        var newViewport = Viewport with { Resolution = resolution };
        SetViewport(newViewport, duration, easing);
    }

    /// <summary>
    /// Zoom to a given resolution with a given point as center
    /// </summary>
    /// <param name="resolution">Resolution to zoom</param>
    /// <param name="centerOfZoomScreen">Center of zoom in screen coordinates. This is the one point in the map that 
    /// stays on the same location while zooming in. 
    /// For instance, in mouse wheel zoom animation the position 
    /// of the mouse pointer can be the center of zoom. Note, that the centerOfZoom is in screen coordinates not 
    /// world coordinates, this is because this is most convenient for the main use case, zoom with the mouse 
    /// position as center.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The easing of the animation when duration is > 0</param>
    public void ZoomTo(double resolution, ScreenPosition centerOfZoomScreen, long duration = -1, Easing? easing = default)
    {
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => ZoomTo(resolution, centerOfZoomScreen, duration, easing));
            return;
        }

        var (centerOfZoomX, centerOfZoomY) = Viewport.ScreenToWorldXY(
            centerOfZoomScreen.X, centerOfZoomScreen.Y);

        var (x, y) = TransformationAlgorithms.CalculateCenterOfMap(
            centerOfZoomX, centerOfZoomY, resolution, Viewport.CenterX, Viewport.CenterY, Viewport.Resolution);
        var newViewport = Viewport with { CenterX = x, CenterY = y, Resolution = resolution };

        SetViewport(newViewport, duration, easing);
    }

    /// <summary>
    /// Zoom in to the next resolution in the Navigator.Resolutions list. Respects ZoomLock.
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomIn(long duration = -1, Easing? easing = default)
    {
        if (ZoomLock)
            return;

        var resolution = ZoomHelper.GetResolutionToZoomIn(Resolutions, Viewport.Resolution);
        ZoomTo(resolution, duration, easing);
    }

    /// <summary>
    /// Zoom out to the next resolution in the Navigator.Resolutions list. Respects ZoomLock.
    /// </summary>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomOut(long duration = -1, Easing? easing = default)
    {
        if (ZoomLock)
            return;

        var resolution = ZoomHelper.GetResolutionToZoomOut(Resolutions, Viewport.Resolution);
        ZoomTo(resolution, duration, easing);
    }

    /// <summary>
    /// Zoom in to a given point
    /// </summary>
    /// <param name="centerOfZoom">Center of zoom. This is the one point in the map that stays on the same location 
    /// while zooming in.For instance, in mouse wheel zoom animation the position of the mouse pointer can be the 
    /// center of zoom.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomIn(ScreenPosition centerOfZoom, long duration = -1, Easing? easing = default)
    {
        var resolution = ZoomHelper.GetResolutionToZoomIn(Resolutions, Viewport.Resolution);
        ZoomTo(resolution, centerOfZoom, duration, easing);
    }

    /// <summary>
    /// Zoom out to a given point
    /// </summary>
    /// <param name="centerOfZoom">Center of zoom. This is the one point in the map that stays on the same location 
    /// while zooming in. For instance, in mouse wheel zoom animation the position of the mouse pointer can be the 
    /// center of zoom.</param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void ZoomOut(ScreenPosition centerOfZoom, long duration = -1, Easing? easing = default)
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
            Logger.Log(LogLevel.Warning, $"Navigator: Zoom level '{level}' is not an index in the range of the resolutions list. " +
                $"The resolutions list is length `{Resolutions.Count}`");
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
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => CenterOn(center, duration, easing));
            return;
        }

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
        SetViewportAnimations(FlyToAnimation.Create(Viewport, center, maxResolution, duration));
    }

    /// <summary>
    /// Change rotation of the viewport
    /// </summary>
    /// <param name="rotation">New rotation in degrees of the viewport></param>
    /// <param name="duration">Duration for animation in milliseconds.</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    public void RotateTo(double rotation, long duration = -1, Easing? easing = default)
    {
        if (!HasExecutedPostponedCalls)
        {
            AddToInitialization(() => RotateTo(rotation, duration, easing));
            return;
        }

        if (RotationLock)
            return;

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
        if (PanLock)
            return;

        SetViewportAnimations(FlingAnimation.Create(velocityX, velocityY, maxDuration, () => OnFetchRequested(ChangeType.Discrete)));
    }

    public void Manipulate(Manipulation? manipulation)
    {
        if (manipulation is null)
            return;

        if (RotationLock)
            manipulation = manipulation with { RotationChange = 0 };
        if (ZoomLock)
            manipulation = manipulation with { ScaleFactor = 1 };
        if (PanLock)
            manipulation = manipulation with { Center = GetScreenCenter(), PreviousCenter = GetScreenCenter() };

        ClearAnimations();

        var viewport = TransformState(Viewport, manipulation);
        SetViewportWithLimit(viewport);
    }

    private ScreenPosition GetScreenCenter() => new ScreenPosition(Viewport.Width * 0.5, Viewport.Height * 0.5);

    private Viewport TransformState(Viewport viewport, Manipulation manipulation)
    {
        var previous = viewport.ScreenToWorld(manipulation.PreviousCenter.X, manipulation.PreviousCenter.Y);
        var current = viewport.ScreenToWorld(manipulation.Center.X, manipulation.Center.Y);

        var scaleFactor = manipulation.ScaleFactor;
        var rotationChange = manipulation.RotationChange;

        if (!RotationLock)
        {
            double virtualRotation = Viewport.Rotation + manipulation.TotalRotationChange;
            rotationChange = RotationSnapper.AdjustRotationDeltaForSnapping(
                manipulation.RotationChange, viewport.Rotation, virtualRotation, UnSnapRotation, ReSnapRotation);
        }

        var newX = viewport.CenterX + previous.X - current.X;
        var newY = viewport.CenterY + previous.Y - current.Y;

        if (scaleFactor == 1 && rotationChange == 0 && viewport.CenterX == newX && viewport.CenterY == newY)
            return viewport;

        if (scaleFactor != 1)
        {
            viewport = viewport with { Resolution = viewport.Resolution / scaleFactor };

            // Calculate current position again with adjusted resolution
            // Zooming should be centered on the place where the map is touched.
            // This is done with the scale correction.
            var scaleCorrectionX = (1 - scaleFactor) * (current.X - viewport.CenterX);
            var scaleCorrectionY = (1 - scaleFactor) * (current.Y - viewport.CenterY);

            newX -= scaleCorrectionX;
            newY -= scaleCorrectionY;
        }

        viewport = viewport with { CenterX = newX, CenterY = newY };

        if (rotationChange != 0)
        {
            // calculate current position again with adjusted resolution
            current = viewport.ScreenToWorld(manipulation.Center.X, manipulation.Center.Y);
            viewport = viewport with { Rotation = viewport.Rotation + rotationChange };
            // calculate current position again with adjusted resolution
            var postRotation = viewport.ScreenToWorld(manipulation.Center.X, manipulation.Center.Y);
            viewport = viewport with
            {
                CenterX = viewport.CenterX - (postRotation.X - current.X),
                CenterY = viewport.CenterY - (postRotation.Y - current.Y)
            };
        }

        return viewport;
    }


    public void SetSize(double width, double height)
    {
        if (Viewport.Width == width && Viewport.Height == height)
            return; // No change in size, no need to update.

        Logger.Log(LogLevel.Information, $"Navigator: SetSize {width} x {height}");
        ClearAnimations();
        SetViewportWithLimit(Viewport with { Width = width, Height = height });
        var wasInitialized = HasExecutedPostponedCalls;
        InitializeIfNeeded();
        if (wasInitialized) // Workaround to prevent double data refresh: Only call when it was already initialized because if it is not then it will be called in Initialize().
            OnFetchRequested(ChangeType.Discrete);
    }

    private void InitializeIfNeeded()
    {
        if (!HasExecutedPostponedCalls && Viewport.HasSize())
            ExecutedPostponedCalls();
    }

    private void OnFetchRequested(ChangeType changeType)
    {
        if (_suppressNotifications)
            return;

        // At the moment we refresh the data on each FetchRequested. Instead we should  always
        // refresh on ChangeType.Discrete, and do throttled requests on ChangeType.Continuous.
        FetchRequested?.Invoke(this, new FetchRequestedEventArgs(changeType));
    }

    private void ClearAnimations()
    {
        if (_animations.Any())
        {
            _animations = [];
        }
    }

    /// <summary>
    /// Property change event
    /// </summary>
    /// <param name="oldViewport">Name of property that changed</param>
    private void OnViewportChanged(Viewport oldViewport, Viewport viewport)
    {
        if (_suppressNotifications)
            return;

        ViewportChanged?.Invoke(this, new ViewportChangedEventArgs(oldViewport, viewport));
    }

    public bool UpdateAnimations()
    {
        if (!_animations.Any()) return false;
        if (_animations.All(a => a.Done))
        {
            ClearAnimations();
            OnFetchRequested(ChangeType.Discrete);
        }
        var animationResult = Animation.UpdateAnimations(Viewport, _animations);

        SetViewportWithLimit(animationResult.State);

        if (ShouldAnimationsBeHaltedBecauseOfLimiting(animationResult.State, Viewport))
        {
            ClearAnimations();
            OnFetchRequested(ChangeType.Discrete);
            return false; // Not running
        }

        return animationResult.IsRunning;
    }

    public void SetViewportAnimations(List<AnimationEntry<Viewport>> animations)
    {
        _animations = animations;
        OnViewportChanged(Viewport, Viewport); // Call OnViewportChanged with current Viewport to trigger the invalidate loop so animations will be updated.
    }

    private void SetViewportWithLimit(Viewport viewport)
    {
        Viewport = Limit(viewport);
    }

    private static bool ShouldAnimationsBeHaltedBecauseOfLimiting(Viewport input, Viewport output)
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

    private Viewport Limit(Viewport goalViewport)
    {
        var limitedViewport = Limiter.Limit(goalViewport, PanBounds, ZoomBounds);

        limitedViewport = LimitXYProportionalToResolution(Viewport, goalViewport, limitedViewport);

        return limitedViewport;
    }

    private Viewport LimitXYProportionalToResolution(
        Viewport originalViewport, Viewport goalViewport, Viewport limitedViewport)
    {
        // From a users experience perspective we want the x/y change to be limited to the same degree
        // as the resolution. This is to prevent the situation where you zoom out while hitting the zoom bounds
        // and you see no change in resolution, but you will see a change in pan.

        if (originalViewport.Resolution <= 0)
            return goalViewport;

        var resolutionLimiting = CalculateResolutionLimiting(
            originalViewport.Resolution, goalViewport.Resolution, limitedViewport.Resolution);

        if (resolutionLimiting > 0)
        {
            var correctionX = (limitedViewport.CenterX - originalViewport.CenterX) * resolutionLimiting;
            var limitedCenterX = limitedViewport.CenterX - correctionX;
            var correctionY = (limitedViewport.CenterY - originalViewport.CenterY) * resolutionLimiting;
            var limitedCenterY = limitedViewport.CenterY - correctionY;
            limitedViewport = limitedViewport with { CenterX = limitedCenterX, CenterY = limitedCenterY };
            // Limit again because this correction could result in x/y values outside of the limit.
            limitedViewport = Limiter.Limit(limitedViewport, PanBounds, ZoomBounds);
        }

        return limitedViewport;
    }

    /// <summary>
    /// Returns a number between 0 and 1 that represents the limiting of the resolution.
    /// </summary>
    /// <param name="originalResolution"></param>
    /// <param name="goalResolution"></param>
    /// <param name="limitedResolution"></param>
    /// <returns></returns>
    private static double CalculateResolutionLimiting(
        double originalResolution, double goalResolution, double limitedResolution)
    {
        var denominator = Math.Abs(goalResolution - originalResolution);

        if (denominator <= double.Epsilon)
            return 0; // There was no limiting because there was no difference at all.

        var numerator = Math.Abs(goalResolution - limitedResolution);

        return (double)(numerator / denominator);
    }

    public void SetViewport(Viewport viewport, long duration = -1, Easing? easing = default)
    {
        if (viewport.Resolution <= 0)
            Logger.Log(LogLevel.Warning, $"Navigator: The Viewport was set but Resolution is {viewport.Resolution}. This is unexpected.");

        if (duration <= 0)
        {
            ClearAnimations();
            SetViewportWithLimit(viewport);
            OnFetchRequested(ChangeType.Discrete);
        }
        else
        {
            // If an animation was already started we do a data refresh for the viewport 
            // at this point. Not entirely sure if there could be too many consecutive animations
            // started to overload to the data refresh.
            if (_animations.Any())
                OnFetchRequested(ChangeType.Continuous);
            SetViewportAnimations(ViewportAnimation.Create(Viewport, viewport, duration, easing));
        }
    }

    /// <summary>
    /// Add a call to a function called before initialization of Viewport to a list
    /// </summary>
    /// <param name="action">Function called before initialization</param>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private void AddToInitialization(Action action)
    {
        // Save state when this function is originally called
        // Add action to initialization list
        _postponedCalls.Add(() =>
        {
            Logger.Log(LogLevel.Information, $"Navigator: Executing postponed call '{MetaDataHelper.GetReadableActionName(action)}'");
            action();
            //Restore old settings of locks
        });
    }

    internal int GetAnimationsCount => _animations.Count();

    /// <summary> Default Resolutions automatically set on Layers changed </summary>
    internal IReadOnlyList<double> DefaultResolutions { get; set; } = [];

    /// <summary> Default Zoom Bounds automatically set on Layers changed </summary>

    internal MMinMax? DefaultZoomBounds
    {
        get => _defaultZoomBounds;
        set
        {
            _defaultZoomBounds = value;
            InitializeIfNeeded();
        }
    }

    /// <summary> Default Pan Bounds automatically set on Layers changed </summary>
    internal MRect? DefaultPanBounds
    {
        get => _defaultPanBounds;
        set
        {
            _defaultPanBounds = value;
            InitializeIfNeeded();
        }
    }
}
