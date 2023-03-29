using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Animations;
using Mapsui.Limiting;

namespace Mapsui;

public interface INavigator
{
    /// <summary>
    /// Sets the extent used to restrict panning. Exactly how this extent affects panning
    /// depends on the implementation of the IViewportLimiter.
    /// </summary>
    MRect? PanExtent { get; set; }
    /// <summary>
    /// A pair of the most extreme resolutions (smallest and biggest). How these extremes affect zooming
    /// depends on the implementation of the IViewportLimiter.
    /// </summary>
    MMinMax? ZoomExtremes { get; set; }
    IViewportLimiter Limiter { get; set; }
    Viewport Viewport { get; }

    event PropertyChangedEventHandler? ViewportChanged;

    /// <summary>
    /// List of resolutions that can be used when going to a new zoom level. In the most common
    /// case these resolutions correspond to the resolutions of the background layer of the map. 
    /// In the Mapsui samples this is usually the openstreetmap layer, but there are also situations
    /// where this is no background layer with resolutions. Or where one app switches between different 
    /// background layers with different resolutions. Also note that when pinch zooming these resolutions 
    /// are not used.
    /// </summary>
    IReadOnlyList<double> Resolutions { get; set; }

    MouseWheelAnimation MouseWheelAnimation { get; }

    /// <summary>
    /// Called each time one of the navigation methods is called
    /// </summary>
    EventHandler? Navigated { get; set; }

    void ZoomInOrOut(int mouseWheelDelta, MPoint centerOfZoom);

    /// <summary>
    /// Navigate center of viewport to center of extent and change resolution
    /// </summary>
    /// <param name="extent">New extent for viewport to show</param>
    /// <param name="boxFit">Scale method to use to determine the resolution</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void NavigateTo(MRect extent, MBoxFit boxFit = MBoxFit.Fit, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Change both center and resolution of the viewport
    /// </summary>
    /// <param name="center">The new center</param>
    /// <param name="resolution">The new resolution</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void NavigateTo(MPoint center, double resolution, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Navigate to a resolution, so such the map uses the fill method
    /// </summary>
    /// <param name="boxFit"></param>
    /// <param name="duration">Duration of animation in millisecondsScale method to use to determine resolution</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomToPanExtent(MBoxFit boxFit = MBoxFit.Fill, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Change resolution of viewport
    /// </summary>
    /// <param name="resolution">New resolution to use</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomTo(double resolution, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Change resolution of viewport about the centerOfZoom
    /// </summary>
    /// <param name="resolution">New resolution to use</param>
    /// /// <param name="centerOfZoom">screen center point to zoom at</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomTo(double resolution, MPoint centerOfZoom, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Change center of viewport
    /// </summary>
    /// <param name="center">New center point of viewport</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void CenterOn(MPoint center, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Change center of viewport to X/Y coordinates
    /// </summary>
    /// <param name="x">X value of the new center</param>
    /// <param name="y">Y value of the new center</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void CenterOn(double x, double y, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Change rotation of viewport
    /// </summary>
    /// <param name="rotation">New rotation in degrees of viewport></param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>

    void RotateTo(double rotation, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Zoom out to the next resolution
    /// </summary>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomOut(long duration = 0, Easing? easing = default);

    /// <summary>
    /// Zoom in to the next resolution
    /// </summary>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomIn(long duration = 0, Easing? easing = default);

    /// <summary>
    /// Zoom in about a given point
    /// </summary>
    /// <param name="centerOfZoom">Center to use for zoom in</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomIn(MPoint centerOfZoom, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Zoom out about a given point
    /// </summary>
    /// <param name="centerOfZoom">Center to use for zoom in</param>
    /// <param name="duration">Duration of animation in milliseconds</param>
    /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
    void ZoomOut(MPoint centerOfZoom, long duration = 0, Easing? easing = default);

    /// <summary>
    /// Animate Fling of viewport
    /// </summary>
    /// <param name="velocityX">Screen VelocityX from SwipedEventArgs></param>
    /// <param name="velocityY">Screen VelocityX from SwipedEventArgs></param>
    /// <param name="maxDuration">Max duration of fling deceleration, changes based on total velocity></param>
    void Fling(double velocityX, double velocityY, long maxDuration);

    void FlyTo(MPoint center, double maxResolution, long duration = 2000);
  
    void SetViewportAnimations(List<AnimationEntry<Viewport>> animations);
  
    void SetSize(double width, double height);
 
    void Pinch(MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution, double deltaRotation = 0);

    void Drag(MPoint positionScreen, MPoint previousPositionScreen);

    bool UpdateAnimations();
   
    void SetViewport(Viewport viewport, long duration = 0, Easing? easing = default);
}
