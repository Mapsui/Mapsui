using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public class Navigator : INavigator
    {
        private readonly Map _map;
        private readonly IViewport _viewport;
        private Animation _animation;
        private double _rotationDelta;
        private Point _animationZoomCenter;

        private static long _defaultDuration = 0;

        /// <summary>
        /// Default value for duration, if nothing is given by the different functions
        /// </summary>
        public static long DefaultDuration
        {
            get => _defaultDuration;
            set
            {
                if (value < 0)
                    _defaultDuration = 0;
                else
                    _defaultDuration = value;
            }
        }

        public EventHandler Navigated { get; set; }

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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit, long duration = -1)
        {
            if (extent == null) return;

            var resolution = ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, _viewport.Width, _viewport.Height, scaleMethod);

            NavigateTo(extent.Centroid, resolution, duration);
        }

        /// <summary>
        /// Navigate to a resolution, so such the map uses the fill method
        /// </summary>
        /// <param name="scaleMethod">Scale method to use to determine resolution</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = -1)
        {
            NavigateTo(_map.Envelope, scaleMethod, duration);
        }

        /// <summary>
        /// Navigate to center and change resolution with animation
        /// </summary>
        /// <param name="center">New center to move to</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void NavigateTo(Point center, double resolution, long duration = -1)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetCenter(center);
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (!_viewport.Center.Equals(center))
                {
                    var entry = new AnimationEntry(
                        start: _viewport.Center,
                        end: (ReadOnlyPoint)center,
                        animationStart: 0,
                        animationEnd: 1,
                        easing: Easing.Linear,
                        tick: CenterTick,
                        final: CenterFinal
                    );
                    animations.Add(entry);
                }

                if (_viewport.Resolution != resolution)
                {
                    var entry = new AnimationEntry(
                        start: _viewport.Resolution,
                        end: resolution,
                        animationStart: 0,
                        animationEnd: 1,
                        easing: Easing.Linear,
                        tick: ResolutionTick,
                        final: ResolutionFinal
                    );
                    animations.Add(entry);
                }

                if (animations.Count == 0)
                    return;

                _animation = new Animation(duration);
                _animation.Entries.AddRange(animations);
                _animation.Start();
            }
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="center">screen center point to zoom at</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomTo(Point center, double resolution, long duration = -1)
        {
            ZoomTo(resolution, center, duration);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomTo(double resolution, long duration = -1)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (_viewport.Resolution == resolution)
                    return;

                var entry = new AnimationEntry(
                    start: _viewport.Resolution,
                    end: resolution,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.Linear,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                _animation = new Animation(duration);
                _animation.Entries.AddRange(animations);
                _animation.Start();
            }
        }

        /// <summary>
        /// Zoom in to the next resolution
        /// </summary>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomIn(long duration = -1)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, duration);
        }

        /// <summary>
        /// Zoom out to the next resolution
        /// </summary>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomOut(long duration = -1)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, duration);
        }

        /// <summary>
        /// Zoom in to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom in</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomIn(Point centerOfZoom, long duration = -1)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, centerOfZoom, duration);
        }

        /// <summary>
        /// Zoom out to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom out</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomOut(Point centerOfZoom, long duration = -1)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, centerOfZoom, duration);
        }

        /// <summary>
        /// Zoom to a given resolution with a given point as center
        /// </summary>
        /// <param name="resolution">Resolution to zoom</param>
        /// <param name="centerOfZoom">Center to use for zoom</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomTo(double resolution, Point centerOfZoom, long duration = -1)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _animationZoomCenter = centerOfZoom;

                var animations = new List<AnimationEntry>();

                if (_viewport.Resolution == resolution)
                    return;

                var entry = new AnimationEntry(
                    start: _viewport.Resolution,
                    end: resolution,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.Linear,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                _animation = new Animation(duration);
                _animation.Entries.AddRange(animations);
                _animation.Start();
            }
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(double x, double y, long duration = -1)
        {
            CenterOn(new Point(x, y), duration);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(Point center, long duration = -1)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetCenter(center);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (_viewport.Center.Equals(center))
                    return;

                var entry = new AnimationEntry(
                    start: _viewport.Center,
                    end: (ReadOnlyPoint)center,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.SinOut,
                    tick: CenterTick,
                    final: CenterFinal
                );
                animations.Add(entry);

                _animation = new Animation(duration);
                _animation.Entries.AddRange(animations);
                _animation.Start();
            }
        }

        /// <summary>
        /// Fly to the given center with zooming out to given resolution and in again
        /// </summary>
        /// <param name="center">Point to fly to</param>
        /// <param name="maxResolution">Maximum resolution to zoom out</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void FlyTo(Point center, double maxResolution, long duration = 2000)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            var halfCenter = new Point(_viewport.Center.X + (center.X - _viewport.Center.X) / 2.0, _viewport.Center.Y + (center.Y - _viewport.Center.Y) / 2.0);
            var resolution = _viewport.Resolution;

            if (duration == 0)
            {
                _viewport.SetCenter(center);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var animations = new List<AnimationEntry>();
                AnimationEntry entry;

                if (!_viewport.Center.Equals(center))
                {
                    entry = new AnimationEntry(
                        start: _viewport.Center,
                        end: (ReadOnlyPoint)center,
                        animationStart: 0,
                        animationEnd: 1,
                        easing: Easing.Linear,
                        tick: CenterTick,
                        final: CenterFinal
                    );
                    animations.Add(entry);
                }

                entry = new AnimationEntry(
                    start: _viewport.Resolution,
                    end: maxResolution,
                    animationStart: 0,
                    animationEnd: 0.5,
                    easing: Easing.SinOut,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                entry = new AnimationEntry(
                    start: maxResolution,
                    end: _viewport.Resolution,
                    animationStart: 0.5,
                    animationEnd: 1,
                    easing: Easing.SinIn,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                _animation = new Animation(duration);
                _animation.Entries.AddRange(animations);
                _animation.Start();
            }
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void RotateTo(double rotation, long duration = -1)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetRotation(rotation);

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                var animations = new List<AnimationEntry>();
                AnimationEntry entry;

                if (_viewport.Rotation == rotation)
                    return;

                entry = new AnimationEntry(
                    start: _viewport.Rotation,
                    end: rotation,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.Linear,
                    tick: RotationTick,
                    final: RotationFinal
                );
                animations.Add(entry);

                _rotationDelta = (double)entry.End - (double)entry.Start;

                if (_rotationDelta < -180.0)
                    _rotationDelta += 360.0;

                if (_rotationDelta > 180.0)
                    _rotationDelta -= 360.0;

                _animation = new Animation(duration);
                _animation.Entries.AddRange(animations);
                _animation.Start();
            }
        }

        /// <summary>
        /// Animate Fling of viewport
        /// </summary>
        /// <param name="velocityX">VelocityX from SwipedEventArgs></param>
        /// <param name="velocityY">VelocityX from SwipedEventArgs></param>
        /// <param name="maxDuration">Maximum duration of fling deceleration></param>
        public void AnimateFling(double velocityX, double velocityY, long maxDuration)
        {
            // Stop any old animation if there is one
            StopRunningAnimation();

            if (maxDuration < 16)
                return;

            velocityX = -velocityX;// reverse as it finger direction is oposite to map movement
            velocityY = -velocityY;// reverse as it finger direction is oposite to map movement

            var magnitudeOfV = Math.Sqrt((velocityX * velocityX) + (velocityY * velocityY));

            var animateMillis = magnitudeOfV / 10;

            if (magnitudeOfV < 100 || animateMillis < 16)
                return;

            if (animateMillis > maxDuration)
                animateMillis = maxDuration;

            var animations = new List<AnimationEntry>();
            AnimationEntry entry;

            entry = new AnimationEntry(
                start: (velocityX, velocityY),
                end: (0d, 0d),
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinIn,
                tick: FlingTick,
                final: FlingFinal
            );
            animations.Add(entry);

            _animation = new Animation((long)animateMillis);
            _animation.Entries.AddRange(animations);
            _animation.Start();
        }

        /// <summary>
        /// Stop all running animations
        /// </summary>
        public void StopRunningAnimation()
        {
            if (_animation != null)
            {
                _animation.Stop(false);
                _animation = null;
                _animationZoomCenter = null;
            }
        }

        private void CenterTick(AnimationEntry entry, double value)
        {
            var x = ((ReadOnlyPoint)entry.Start).X + (((ReadOnlyPoint)entry.End).X - ((ReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            var y = ((ReadOnlyPoint)entry.Start).Y + (((ReadOnlyPoint)entry.End).Y - ((ReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);

            // Set new values
            _viewport.SetCenter(x, y);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void CenterFinal(AnimationEntry entry)
        {
            _viewport.SetCenter((ReadOnlyPoint)entry.End);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void ResolutionTick(AnimationEntry entry, double value)
        {
            if (_animationZoomCenter != null)
            {
                // 1) Temporarily center on the center of zoom
                _viewport.SetCenter(_viewport.ScreenToWorld(_animationZoomCenter));
            }

            var r = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);

            // 2) Then zoom 
            _viewport.SetResolution(r);

            if (_animationZoomCenter != null)
            {
                // 3) Then move the temporary center of the map back to the mouse position
                _viewport.SetCenter(_viewport.ScreenToWorld(
                    _viewport.Width - _animationZoomCenter.X,
                    _viewport.Height - _animationZoomCenter.Y));
            }

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void ResolutionFinal(AnimationEntry entry)
        {
            if (_animationZoomCenter != null)
            {
                // 1) Temporarily center on the center of zoom
                _viewport.SetCenter(_viewport.ScreenToWorld(_animationZoomCenter));
            }

            // 2) Then zoom 
            _viewport.SetResolution((double)entry.End);

            if (_animationZoomCenter != null)
            {
                // 3) Then move the temporary center of the map back to the mouse position
                _viewport.SetCenter(_viewport.ScreenToWorld(
                    _viewport.Width - _animationZoomCenter.X,
                    _viewport.Height - _animationZoomCenter.Y));
            }

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void RotationTick(AnimationEntry entry, double value)
        {
            var r = (double)entry.Start + _rotationDelta * entry.Easing.Ease(value);

            // Set new value
            _viewport.SetRotation(r);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void RotationFinal(AnimationEntry entry)
        {
            _viewport.SetRotation((double)entry.End);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void FlingTick(AnimationEntry entry, double value)
        {
            var timeAmount = 16 / 1000d; // 16 milli

            var (velocityX, velocityY) = ((double, double))entry.Start;

            var xMovement = velocityX * (1d - entry.Easing.Ease(value)) * timeAmount;
            var yMovement = velocityY * (1d - entry.Easing.Ease(value)) * timeAmount;

            if (xMovement.IsNanOrInfOrZero())
                xMovement = 0;
            if (yMovement.IsNanOrInfOrZero())
                yMovement = 0;

            if (xMovement == 0 && yMovement == 0)
                return;

            var previous = _viewport.ScreenToWorld(0, 0);
            var current = _viewport.ScreenToWorld(xMovement, yMovement);

            var xDiff = current.X - previous.X;
            var yDiff = current.Y - previous.Y;

            var newX = _viewport.Center.X + xDiff;
            var newY = _viewport.Center.Y + yDiff;

            _viewport.SetCenter(newX, newY);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void FlingFinal(AnimationEntry entry)
        {
            // Nothing to do
        }
    }
}
