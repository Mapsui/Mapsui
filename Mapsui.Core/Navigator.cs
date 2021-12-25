using System;
using Mapsui.Geometries;
using Mapsui.Utilities;
using Mapsui.ViewportAnimations;

namespace Mapsui
{
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
            Navigated?.Invoke(this, ChangeType.Discrete);

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
            Navigated?.Invoke(this, ChangeType.Discrete);
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
            var centerOfMap = CalculateCenterOfMap(centerOfZoom, resolution);
            _viewport.SetCenterAndResolution(centerOfMap.X, centerOfMap.Y, resolution, duration, easing);
            Navigated?.Invoke(this, ChangeType.Discrete);

        }

        /// <summary>
        /// Calculates the new CenterOfMap based on the CenterOfZoom and the new resolution.
        /// The CenterOfZoom is not the same as the CenterOfMap. CenterOfZoom is the one place in
        /// the map that stays on the same location when zooming. In Mapsui is can be equal to the 
        /// CenterOfMap, for instance when using the +/- buttons. When using mouse wheel zoom the
        /// CenterOfZoom is the location of the mouse. 
        /// </summary>
        /// <param name="centerOfZoom"></param>
        /// <param name="newResolution"></param>
        /// <returns></returns>
        private MReadOnlyPoint CalculateCenterOfMap(MPoint centerOfZoom, double newResolution)
        {
            centerOfZoom = _viewport.ScreenToWorld(centerOfZoom);
            var ratio = newResolution / _viewport.Resolution;

            return new MReadOnlyPoint(
                centerOfZoom.X - (centerOfZoom.X - _viewport.Center.X) * ratio,
                centerOfZoom.Y - (centerOfZoom.Y - _viewport.Center.Y) * ratio);
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
            Navigated?.Invoke(this, ChangeType.Discrete);
        }

        /// <summary>
        /// Fly to the given center with zooming out to given resolution and in again
        /// </summary>
        /// <param name="center">MPoint to fly to</param>
        /// <param name="maxResolution">Maximum resolution to zoom out</param>
        /// <param name="duration">Duration for animation in milliseconds.</param>
        public void FlyTo(MPoint center, double maxResolution, long duration = 500)
        {
            _viewport.SetAnimations(FlyToAnimation.Create(_viewport, center, maxResolution, duration));
            Navigated?.Invoke(this, ChangeType.Discrete);
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

            // Todo: call Navigated with the end rotation
            Navigated?.Invoke(this, ChangeType.Discrete);
        }

        /// <summary>
        /// Animate Fling of viewport
        /// </summary>
        /// <param name="velocityX">VelocityX from SwipedEventArgs></param>
        /// <param name="velocityY">VelocityX from SwipedEventArgs></param>
        /// <param name="maxDuration">Maximum duration of fling deceleration></param>
        public void FlingWith(double velocityX, double velocityY, long maxDuration)
        {
            _viewport.SetAnimations(FlingAnimation.Create(_viewport, velocityX, velocityY, maxDuration));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        ~Navigator()
        {
            Dispose(false);
        }
    }
}
