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
        private ViewportAnimation _animation = new ViewportAnimation();
        private double _rotationDelta;
        private static long _defaultDuration = 0;
        AnimationEntry animationRotation;
        AnimationEntry animationCenter;
        AnimationEntry animationResolution;

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

            // Idea:
            // Add animations up front for the 3 variables we want to animate
            // 1. Center
            // 2. Resolution
            // 3. Rotation
            // Animation of the viewport should only go through these variables.
            // Problem: Two step animation like we use with fly over.

            var animations = new List<AnimationEntry>();

            animationCenter = new AnimationEntry(
                start: null,
                end: null,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinInOut,
                tick: CenterTick,
                final: CenterFinal
            );
            animations.Add(animationCenter);

            animationResolution = new AnimationEntry(
                start: null,
                end: null,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.CubicInOut,
                tick: ResolutionTick,
                final: ResolutionFinal
            );
            animations.Add(animationResolution);

            animationRotation = new AnimationEntry(
                start: null,
                end: null,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinInOut,
                tick: RotationTick,
                final: RotationFinal
            );

            animations.Add(animationRotation);

            _animation.Entries.AddRange(animations);

            _animation.Ticked += (s, e) => Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determine resolution</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit, long duration = -1, Easing easing = default)
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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = -1, Easing easing = default)
        {
            NavigateTo(_map.Envelope, scaleMethod, duration, easing);
        }

        /// <summary>
        /// Navigate to center and change resolution with animation
        /// </summary>
        /// <param name="center">New center to move to</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void NavigateTo(Point center, double resolution, long duration = -1, Easing easing = default)
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
                if (!_viewport.Center.Equals(center))
                {
                    animationCenter.Start = _viewport.Center;
                    animationCenter.End = (ReadOnlyPoint)center;
                    animationCenter.AnimationStart = 0;
                    animationCenter.AnimationEnd = 1;
                    animationCenter.Easing = easing ?? Easing.SinInOut;
                    animationCenter.Enabled = true;
                }

                if (_viewport.Resolution != resolution)
                {
                    animationResolution.Start = _viewport.Resolution;
                    animationResolution.End = resolution;
                    animationResolution.AnimationStart = 0;
                    animationResolution.AnimationEnd = 1;
                    animationResolution.Easing = easing ?? Easing.SinInOut;
                    animationResolution.Enabled = true;
                }

                animationRotation.Enabled = false;
                _animation.Duration = duration;
                _animation.Start();
            }
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="center">screen center point to zoom at</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomTo(Point center, double resolution, long duration = -1, Easing easing = default)
        {
            ZoomTo(resolution, center, duration, easing);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomTo(double resolution, long duration = -1, Easing easing = default)
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
                if (_viewport.Resolution == resolution)
                    return;

                animationResolution.Start = _viewport.Resolution;
                animationResolution.End = resolution;
                animationResolution.AnimationStart = 0;
                animationResolution.AnimationEnd = 1;
                animationResolution.Easing = easing ?? Easing.SinInOut;
                animationResolution.Enabled = true;

                _animation.Duration = duration;
                _animation.Start();
            }
        }

        /// <summary>
        /// Zoom to a given resolution with a given point as center
        /// </summary>
        /// <param name="resolution">Resolution to zoom</param>
        /// <param name="centerOfZoom">Center to use for zoom</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomTo(double resolution, Point centerOfZoom, long duration = -1, Easing easing = default)
        {
            // Problem: The only way we can properly animate to resolution and centerOfZoom if we 
            // animate them both independent. So, don;t use the 3 steps, but calculate the center in
            // one step. For this we need some changes to ScreenToWorld. Move core logic to a utilities
            // class so we can call it with the target resolution.

            // Stop any old animation if there is one
            StopRunningAnimation();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                if (centerOfZoom != null)
                {
                    // 1) Temporarily center on the center of zoom
                    _viewport.SetCenter(_viewport.ScreenToWorld(centerOfZoom));
                }

                // 2) Then zoom 
                _viewport.SetResolution((double)resolution);

                if (centerOfZoom != null)
                {
                    // 3) Then move the temporary center of the map back to the mouse position
                    _viewport.SetCenter(_viewport.ScreenToWorld(
                        _viewport.Width - centerOfZoom.X,
                        _viewport.Height - centerOfZoom.Y));
                }

                Navigated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (!_viewport.Center.Equals(centerOfZoom))
                {
                    animationCenter.Start = _viewport.Center;
                    animationCenter.End = (ReadOnlyPoint)centerOfZoom;
                    animationCenter.AnimationStart = 0;
                    animationCenter.AnimationEnd = 1;
                    animationCenter.Easing = Easing.SinInOut;
                    animationCenter.Enabled = true;
                }

                if (_viewport.Resolution == resolution)
                    return;

                animationResolution.Start = _viewport.Resolution;
                animationResolution.End = resolution;
                animationResolution.AnimationStart = 0;
                animationResolution.AnimationEnd = 1;
                animationResolution.Easing = easing;
                animationResolution.Enabled = true;

                _animation.Duration = duration;
                _animation.Start();
            }
        }

        /// <summary>
        /// Zoom in to the next resolution
        /// </summary>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomIn(long duration = -1, Easing easing = default)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, duration, easing);
        }

        /// <summary>
        /// Zoom out to the next resolution
        /// </summary>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomOut(long duration = -1, Easing easing = default)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, duration, easing);
        }

        /// <summary>
        /// Zoom in to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom in</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomIn(Point centerOfZoom, long duration = -1, Easing easing = default)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, centerOfZoom, duration, easing);
        }

        /// <summary>
        /// Zoom out to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom out</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void ZoomOut(Point centerOfZoom, long duration = -1, Easing easing = default)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
            ZoomTo(resolution, centerOfZoom, duration, easing);
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(double x, double y, long duration = -1, Easing easing = default)
        {
            CenterOn(new Point(x, y), duration, easing);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(Point center, long duration = -1, Easing easing = default)
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
                if (_viewport.Center.Equals(center))
                    return;

                animationCenter.Start = _viewport.Center;
                animationCenter.End = (ReadOnlyPoint)center;
                animationCenter.AnimationStart = 0;
                animationCenter.AnimationEnd = 1;
                animationCenter.Easing = easing ?? Easing.SinOut;
                animationCenter.Enabled = true;

                _animation.Duration = duration;
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
                if (!_viewport.Center.Equals(center))
                {
                    animationCenter.Start = _viewport.Center;
                    animationCenter.End = (ReadOnlyPoint)center;
                    animationCenter.AnimationStart = 0;
                    animationCenter.AnimationEnd = 1;
                    animationCenter.Easing = Easing.SinInOut;
                    animationCenter.Enabled = true;
                }

                // todo: find solution for flying up to max
                //animationResolution.Start = _viewport.Resolution;
                //animationResolution.End = maxResolution;
                //animationResolution.AnimationStart = 0;
                //animationResolution.AnimationEnd = 0.5;
                //animationResolution.Easing = Easing.SinIn;
                //animationResolution.Enabled = true;

                animationResolution.Start = maxResolution;
                animationResolution.End = _viewport.Resolution;
                animationResolution.AnimationStart = 0.0;
                animationResolution.AnimationEnd = 1;
                animationResolution.Easing = Easing.SinIn;
                animationResolution.Enabled = true;

                _animation.Duration = duration;
                _animation.Start();
            }
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void RotateTo(double rotation, long duration = -1, Easing easing = default)
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
                if (_viewport.Rotation == rotation)
                    return;


                animationRotation.Start = _viewport.Rotation;
                animationRotation.End = rotation;
                animationRotation.AnimationStart = 0;
                animationRotation.AnimationEnd = 1;
                animationRotation.Easing = easing ?? Easing.SinInOut;

                _rotationDelta = (double)animationRotation.End - (double)animationRotation.Start;

                if (_rotationDelta < -180.0)
                    _rotationDelta += 360.0;

                if (_rotationDelta > 180.0)
                    _rotationDelta -= 360.0;


                _animation.Duration = duration;
                _animation.Start();
            }
        }

        /// <summary>
        /// Animate Fling of viewport
        /// </summary>
        /// <param name="velocityX">VelocityX from SwipedEventArgs></param>
        /// <param name="velocityY">VelocityX from SwipedEventArgs></param>
        /// <param name="maxDuration">Maximum duration of fling deceleration></param>
        public void FlingWith(double velocityX, double velocityY, long maxDuration)
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

            // todo: Calculate the final distance traveled given the initial velocity. 
            // This was my first google hit on the topic: https://www.youtube.com/watch?v=8fAYAcr1zJU
            // Perhaps the duration needs to be calculated.
            var targetCenter = new ReadOnlyPoint(_viewport.Center.X + velocityX * 1000, _viewport.Center.Y - velocityY * 1000);

            animationCenter.Start = _viewport.Center;
            animationCenter.End = targetCenter;
            animationCenter.AnimationStart = 0;
            animationCenter.AnimationEnd = 1;
            animationCenter.Easing = Easing.QuarticOut;
            animationCenter.Enabled = true;

            _animation.Duration = (long)animateMillis;
            _animation.Start();
        }

        /// <summary>
        /// Stop all running animations
        /// </summary>
        public void StopRunningAnimation()
        {
            animationCenter.Enabled = false;
            animationResolution.Enabled = false;
            animationRotation.Enabled = false;

            _animation.Stop(false);
        }

        private void CenterTick(AnimationEntry entry, double value)
        {
            if (entry.Start == null) return; //!!!

            var x = ((ReadOnlyPoint)entry.Start).X + (((ReadOnlyPoint)entry.End).X - ((ReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            var y = ((ReadOnlyPoint)entry.Start).Y + (((ReadOnlyPoint)entry.End).Y - ((ReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);

            // Set new values
            _viewport.SetCenter(x, y);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void CenterFinal(AnimationEntry entry)
        {
            if (entry.Start == null) return; //!!!

            _viewport.SetCenter((ReadOnlyPoint)entry.End);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void ResolutionTick(AnimationEntry entry, double value)
        {
            var r = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);

            //!!!_viewport.SetResolution(r);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void ResolutionFinal(AnimationEntry entry)
        {
            _viewport.SetResolution((double)entry.End);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void RotationTick(AnimationEntry entry, double value)
        {
            if (entry.Start == null) return; //!!!

            var r = (double)entry.Start + _rotationDelta * entry.Easing.Ease(value);

            // Set new value
            _viewport.SetRotation(r);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void RotationFinal(AnimationEntry entry)
        {
            if (entry.End == null) return;

            _viewport.SetRotation((double)entry.End);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateAnimation()
        {
            _animation.Tick();
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
    }
}
