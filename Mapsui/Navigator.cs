using System;
using System.Diagnostics;
using System.Timers;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public class Navigator : INavigator
    {
        private readonly Map _map;
        private readonly IViewport _viewport;

        // Objects for animation
        private readonly Timer _timer;
        private Stopwatch _stopwatch;
        private long _animationStart;
        private long _animationDuration;
        private ReadOnlyPoint _startCenter;
        private ReadOnlyPoint _endCenter;
        private double _startResolution;
        private double _endResolution;
        private double _deltaX;
        private double _deltaY;
        private double _deltaResolution;
        private Easing _easingCenter;
        private Easing _easingResolution;
        private Action _endAnimationCallback;

        public EventHandler Navigated { get; set; } 

        public Navigator(Map map, IViewport viewport)
        {
            _map = map;
            _viewport = viewport;

            // Create timer for animation
            _timer = new Timer();
            _timer.Interval = 16;
            _timer.AutoReset = true;
            _timer.Elapsed += HandleTimerElapse;
        }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determine resolution</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            if (extent == null) return;

            var resolution = ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, _viewport.Width, _viewport.Height, scaleMethod);

            NavigateTo(extent.Centroid, resolution);
        }

        /// <summary>
        /// Navigate to a resolution, so such the map uses the fill method
        /// </summary>
        /// <param name="scaleMethod"></param>
        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill)
        {
            NavigateTo(_map.Envelope, scaleMethod);
        }

        /// <inheritdoc />
        public void NavigateTo(Point center, double resolution)
        {
            NavigateTo(center, resolution, 0, null, null);
        }

        /// <summary>
        /// Navigate to  center and change resolution with animation
        /// </summary>
        /// <param name="center">New center to move to</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        /// <param name="easingCenter">Function for easing of center change</param>
        /// <param name="easingResolution">Function for easing of resolution change</param>
        public void NavigateTo(Point center, double resolution, long duration = 0, Easing easingCenter = null, Easing easingResolution = null)
        {
            if (duration == 0)
            {
                _viewport.SetCenter(center);
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StartAnimation(duration, center, resolution, easingCenter, easingResolution, callback: () => Navigated?.Invoke(this, EventArgs.Empty));
            }
        }

        /// <inheritdoc />
        public void ZoomTo(double resolution)
        {
            ZoomTo(resolution, 0, null);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        /// <param name="easing">Function for easing</param>
        public void ZoomTo(double resolution, long duration = 0, Easing easing = null)
        {
            if (duration == 0)
            {
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StartAnimation(duration, toResolution: resolution, easingResolution: easing, callback: () => Navigated?.Invoke(this, EventArgs.Empty));
            }
        }

        /// <summary>
        /// Zoom in to the next resolution
        /// </summary>
        public void ZoomIn()
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution);
        }

        /// <summary>
        /// Zoom out to the next resolution
        /// </summary>
        public void ZoomOut()
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution);
        }

        /// <summary>
        /// Zoom in to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom in</param>
        public void ZoomIn(Point centerOfZoom)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);
            ZoomTo(resolution, centerOfZoom);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zoom out to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom out</param>
        public void ZoomOut(Point centerOfZoom)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
            ZoomTo(resolution, centerOfZoom);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zoom to a given resolution with a given point as center
        /// </summary>
        /// <param name="resolution">Resolution to zoom</param>
        /// <param name="centerOfZoom">Center to use for zoom</param>
        public void ZoomTo(double resolution, Point centerOfZoom)
        {
            // 1) Temporarily center on the center of zoom
            _viewport.SetCenter(_viewport.ScreenToWorld(centerOfZoom));

            // 2) Then zoom 
            _viewport.SetResolution(resolution);

            // 3) Then move the temporary center of the map back to the mouse position
            _viewport.SetCenter(_viewport.ScreenToWorld(
                _viewport.Width - centerOfZoom.X,
                _viewport.Height - centerOfZoom.Y));

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void CenterOn(double x, double y)
        {
            CenterOn(x, y, 0, null);
        }

        /// <inheritdoc />
        public void CenterOn(Point center)
        {
            CenterOn(center.X, center.Y, 0, null);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(Point center, long duration = 0, Easing easing = null)
        {
            CenterOn(center.X, center.Y, duration, easing);
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(double x, double y, long duration = 0, Easing easing = null)
        {
            if (duration == 0)
            {
                _viewport.SetCenter(x, y);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                StartAnimation(duration, new Point(x, y), easingCenter: easing, callback: () => Navigated?.Invoke(this, EventArgs.Empty));
            }
        }

        /// <summary>
        /// Fly to the given center with zooming out to given resolution and in again
        /// </summary>
        /// <param name="center">Point to fly to</param>
        /// <param name="maxResolution">Maximum resolution to zoom out</param>
        /// <param name="duration">Duration for animation in milliseconds</param>
        public void FlyTo(Point center, double maxResolution, long duration = 2000)
        {
            var halfCenter = new Point(_viewport.Center.X + (center.X - _viewport.Center.X) / 2.0, _viewport.Center.Y + (center.Y - _viewport.Center.Y) / 2.0);
            var resolution = _viewport.Resolution;
            StartAnimation(duration / 2, halfCenter, maxResolution, Easing.Linear, Easing.SinOut, () => StartAnimation(duration / 2, center, resolution, Easing.Linear, Easing.SinIn));
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        public void RotateTo(double rotation)
        {
            _viewport.SetRotation(rotation);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Start an animation
        /// </summary>
        /// <param name="milliseconds">Duration of this animation</param>
        /// <param name="toCenter">Position where to go to</param>
        /// <param name="toResolution">Resolution where to go to</param>
        /// <param name="easingCenter">Easing function for position change</param>
        /// <param name="easingResolution">Easing function for resolution change</param>
        /// <param name="callback">Callback function which is called at the end</param>
        private void StartAnimation(long milliseconds, ReadOnlyPoint toCenter = null, double? toResolution = null, Easing easingCenter = null, Easing easingResolution = null, Action callback = null)
        {
            if (_timer != null && _timer.Enabled)
            {
                StopAnimation(false);
            }

            _startCenter = _viewport.Center;
            _startResolution = _viewport.Resolution;

            _endCenter = toCenter ?? _viewport.Center;
            _endResolution = (double)(toResolution ?? _viewport.Resolution);

            _easingCenter = easingCenter ?? Easing.Linear;
            _easingResolution = easingResolution ?? Easing.Linear;

            _endAnimationCallback = callback;

            _deltaX = _endCenter.X - _viewport.Center.X;
            _deltaY = _endCenter.Y - _viewport.Center.Y;
            _deltaResolution = _endResolution - _viewport.Resolution;

            // Animation in ticks;
            _animationDuration = milliseconds * Stopwatch.Frequency / 1000;

            _stopwatch = Stopwatch.StartNew();
            _animationStart = _stopwatch.ElapsedTicks;
            _timer.Start();
        }

        /// <summary>
        /// Stop a running animation if there is one
        /// </summary>
        /// <param name="gotoEnd"></param>
        private void StopAnimation(bool gotoEnd)
        {
            if (!_timer.Enabled)
                return;

            _timer.Stop();
            _stopwatch.Stop();

            if (gotoEnd)
            {
                _viewport.SetResolution(_endResolution);
                _viewport.SetCenter(_endCenter);
            }

            _endAnimationCallback?.Invoke();
        }

        /// <summary>
        /// Timer tick for animation
        /// </summary>
        /// <param name="sender">Sender of this tick</param>
        /// <param name="e">Timer tick arguments</param>
        private void HandleTimerElapse(object sender, ElapsedEventArgs e)
        {
            double ticks = _stopwatch.ElapsedTicks - _animationStart;
            var value = ticks / _animationDuration;

            if (value >= 1.0)
            {
                StopAnimation(true);
                return;
            }

            // Calc new values
            var x = _startCenter.X + _deltaX * _easingCenter.Ease(value);
            var y = _startCenter.Y + _deltaY * _easingCenter.Ease(value);
            var r = _startResolution + _deltaResolution * _easingResolution.Ease(value);

            // Set new values
            _viewport.SetResolution(r);
            _viewport.SetCenter(x, y);
            Navigated?.Invoke(this, EventArgs.Empty);
        }
    }
}
