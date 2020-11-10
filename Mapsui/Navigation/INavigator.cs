﻿using System;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public interface INavigator
    {
        /// <summary>
        /// Called each time one of the navigation methods is called
        /// </summary>
        EventHandler<ChangeType> Navigated { get; set; }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determin resolution</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit, long duration = 0, Easing easing = default);

        /// <summary>
        /// Change both center and resolution of the viewport
        /// </summary>
        /// <param name="center">The new center</param>
        /// <param name="resolution">The new resolution</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void NavigateTo(Point center, double resolution, long duration = 0, Easing easing = default);

        /// <summary>
        /// Navigate to a resolution, so such the map uses the fill method
        /// </summary>
        /// <param name="scaleMethod"></param>
        /// <param name="duration">Duration of animation in millisecondsScale method to use to determine resolution</param>
        void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = 0, Easing easing = default);

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void ZoomTo(double resolution, long duration = 0, Easing easing = default);

        /// <summary>
        /// Change resolution of viewport about the centerOfZoom
        /// </summary>
        /// <param name="center">screen center point to zoom at</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void ZoomTo(double resolution, Point centerOfZoom, long duration = 0, Easing easing = default);

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void CenterOn(Point center, long duration = 0, Easing easing = default);

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void CenterOn(double x, double y, long duration = 0, Easing easing = default);

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void RotateTo(double rotation, long duration = 0, Easing easing = default);

        /// <summary>
        /// Zoom out to the next resolution
        /// </summary>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void ZoomOut(long duration = 0, Easing easing = default);

        /// <summary>
        /// Zoom in to the next resolution
        /// </summary>
        /// <param name="duration">Duration of animation in milliseconds</param>
        void ZoomIn(long duration = 0, Easing easing = default);

        /// <summary>
        /// Zoom in about a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom in</param>
        void ZoomIn(Point centerOfZoom, long duration = 0, Easing easing = default);

        /// <summary>
        /// Zoom out about a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom in</param>
        void ZoomOut(Point centerOfZoom, long duration = 0, Easing easing = default);

        /// <summary>
        /// Animate Fling of viewport
        /// </summary>
        /// <param name="velocityX">Screen VelocityX from SwipedEventArgs></param>
        /// <param name="velocityY">Screen VelocityX from SwipedEventArgs></param>
        /// <param name="maxDuration">Max duration of fling deceleration, changes based on total velocity></param>
        void FlingWith(double velocityX, double velocityY, long maxDuration);

        /// <summary>
        /// Stop any current animation if it is running
        /// </summary>
        void StopRunningAnimation();

        void UpdateAnimations();
    }
}
