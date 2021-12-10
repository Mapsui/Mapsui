﻿using System;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    /// <summary>
    /// A Navigator is used to change the visible part (Viewport) of a MapControl
    /// </summary>
    public class Navigator : INavigator, IAnimatable
    {
        private readonly Map _map;
        private readonly IViewport _viewport;
        private double _rotationDelta;
        private List<AnimationEntry> _animations = new();

        private static long _defaultDuration;

        /// <summary>
        /// Default value for duration, if nothing is given by the different functions
        /// </summary>
        public static long DefaultDuration
        {
            get => _defaultDuration;
            set => _defaultDuration = value < 0 ? 0 : value;
        }

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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = -1, Easing? easing = default)
        {
            NavigateTo(_map.Extent, scaleMethod, duration, easing);
        }

        /// <summary>
        /// Navigate to center and change resolution with animation
        /// </summary>
        /// <param name="center">New center to move to</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
        public void NavigateTo(MPoint? center, double resolution, long duration = -1, Easing? easing = default)
        {
            if (center == null)
                return;

            // Stop any old animation if there is one
            StopRunningAnimations();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetResolution(resolution);
                _viewport.SetCenter(center);

                Navigated?.Invoke(this, ChangeType.Discrete);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (!center.Equals(_viewport.Center))
                {
                    var entry = new AnimationEntry(
                        name: "Moving",
                        start: _viewport.Center,
                        end: (MReadOnlyPoint)center,
                        animationStart: 0,
                        animationEnd: 1,
                        easing: easing ?? Easing.SinInOut,
                        tick: CenterTick,
                        final: CenterFinal
                    );
                    animations.Add(entry);
                }

                if (_viewport.Resolution != resolution)
                {
                    var entry = new AnimationEntry(
                        name: "Zooming",
                        start: _viewport.Resolution,
                        end: resolution,
                        animationStart: 0,
                        animationEnd: 1,
                        easing: easing ?? Easing.SinInOut,
                        tick: ResolutionTick,
                        final: ResolutionFinal
                    );
                    animations.Add(entry);
                }

                if (animations.Count == 0)
                    return;


                Animation.Start(animations, duration);

                _animations = animations;
            }
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
        public void ZoomTo(double resolution, long duration = -1, Easing? easing = default)
        {
            // Stop any old animation if there is one
            StopRunningAnimations();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, ChangeType.Discrete);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (_viewport.Resolution == resolution)
                    return;

                var entry = new AnimationEntry(
                    name: "Zooming",
                    start: _viewport.Resolution,
                    end: resolution,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: easing ?? Easing.SinInOut,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                Animation.Start(animations, duration);

                _animations = animations;
            }
        }

        /// <summary>
        /// Zoom to a given resolution with a given point as center
        /// </summary>
        /// <param name="resolution">Resolution to zoom</param>
        /// <param name="centerOfZoom">Center of zoom. This is the one point in the map that stays on the same location while zooming in.
        /// For instance, in mouse wheel zoom animation the position of the mouse pointer can be the center of zoom.</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">The easing of the animation when duration is > 0</param>
        public void ZoomTo(double resolution, MPoint centerOfZoom, long duration = -1, Easing? easing = default)
        {
            // todo: Perhaps centerOfZoom should be passed in in world coordinates. 
            // This means the caller has to do the conversion, but it is more consistent since the centerOfMap 
            // arguments to the navigator are in World coordinates as well.

            // Stop any old animation if there is one
            StopRunningAnimations();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                // The order matters because SetCenter depends on the current resolution
                _viewport.SetCenter(CalculateCenterOfMap(centerOfZoom, resolution));
                _viewport.SetResolution(resolution);

                Navigated?.Invoke(this, ChangeType.Discrete);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                var centerEntry = new AnimationEntry(
                    name: "Moving",
                    start: _viewport.Center,
                    end: CalculateCenterOfMap(centerOfZoom, resolution),
                    animationStart: 0,
                    animationEnd: 1,
                    easing: Easing.QuarticOut,
                    tick: CenterTick,
                    final: CenterFinal
                );
                animations.Add(centerEntry);

                if (_viewport.Resolution == resolution)
                    return;

                var entry = new AnimationEntry(
                    name: "Zooming",
                    start: _viewport.Resolution,
                    end: resolution,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: easing ?? Easing.QuarticOut,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                Animation.Start(animations, duration);

                _animations = animations;
            }
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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
        public void ZoomIn(long duration = -1, Easing? easing = default)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

            ZoomTo(resolution, duration, easing);
        }

        /// <summary>
        /// Zoom out to the next resolution
        /// </summary>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
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
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(double x, double y, long duration = -1, Easing? easing = default)
        {
            CenterOn(new MPoint(x, y), duration, easing);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(MPoint center, long duration = -1, Easing? easing = default)
        {
            // Stop any old animation if there is one
            StopRunningAnimations();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetCenter(center);

                Navigated?.Invoke(this, ChangeType.Discrete);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (center.Equals(_viewport.Center))
                    return;

                var entry = new AnimationEntry(
                    name: "Moving",
                    start: _viewport.Center,
                    end: (MReadOnlyPoint)center,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: easing ?? Easing.SinOut,
                    tick: CenterTick,
                    final: CenterFinal
                );
                animations.Add(entry);

                Animation.Start(animations, duration);

                _animations = animations;
            }
        }

        /// <summary>
        /// Fly to the given center with zooming out to given resolution and in again
        /// </summary>
        /// <param name="center">MPoint to fly to</param>
        /// <param name="maxResolution">Maximum resolution to zoom out</param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        public void FlyTo(MPoint center, double maxResolution, long duration = 2000)
        {
            // Stop any old animation if there is one
            StopRunningAnimations();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetCenter(center);

                Navigated?.Invoke(this, ChangeType.Discrete);
            }
            else
            {
                var animations = new List<AnimationEntry>();
                AnimationEntry entry;

                if (!center.Equals(_viewport.Center))
                {
                    entry = new AnimationEntry(
                        name: "Moving",
                        start: _viewport.Center,
                        end: (MReadOnlyPoint)center,
                        animationStart: 0,
                        animationEnd: 1,
                        easing: Easing.SinInOut,
                        tick: CenterTick,
                        final: CenterFinal
                    );
                    animations.Add(entry);
                }

                entry = new AnimationEntry(
                    name: "Zooming",
                    start: _viewport.Resolution,
                    end: maxResolution,
                    animationStart: 0,
                    animationEnd: 0.5,
                    easing: Easing.SinIn,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                entry = new AnimationEntry(
                    name: "Zooming",
                    start: maxResolution,
                    end: _viewport.Resolution,
                    animationStart: 0.5,
                    animationEnd: 1,
                    easing: Easing.SinIn,
                    tick: ResolutionTick,
                    final: ResolutionFinal
                );
                animations.Add(entry);

                Animation.Start(animations, duration);

                _animations = animations;
            }
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        /// <param name="duration">Duration for animation in milliseconds. If less then 0, then <see cref="DefaultDuration"/> is used.</param>
        /// <param name="easing">The type of easing function used to transform from begin tot end state</param>
        public void RotateTo(double rotation, long duration = -1, Easing? easing = default)
        {
            // Stop any old animation if there is one
            StopRunningAnimations();

            duration = duration < 0 ? _defaultDuration : duration;

            if (duration == 0)
            {
                _viewport.SetRotation(rotation);

                Navigated?.Invoke(this, ChangeType.Discrete);
            }
            else
            {
                var animations = new List<AnimationEntry>();

                if (_viewport.Rotation == rotation)
                    return;

                var entry = new AnimationEntry(
                    name: "Rotation",
                    start: _viewport.Rotation,
                    end: rotation,
                    animationStart: 0,
                    animationEnd: 1,
                    easing: easing ?? Easing.SinInOut,
                    tick: RotationTick,
                    final: RotationFinal
                );
                animations.Add(entry);

                _rotationDelta = (double)entry.End - (double)entry.Start;

                if (_rotationDelta < -180.0)
                    _rotationDelta += 360.0;

                if (_rotationDelta > 180.0)
                    _rotationDelta -= 360.0;

                Animation.Start(animations, duration);

                _animations = animations;
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
            StopRunningAnimations();

            if (maxDuration < 16)
                return;

            velocityX = -velocityX;// reverse as it finger direction is opposite to map movement
            velocityY = -velocityY;// reverse as it finger direction is opposite to map movement

            var magnitudeOfV = Math.Sqrt((velocityX * velocityX) + (velocityY * velocityY));

            var animateMillis = magnitudeOfV / 10;

            if (magnitudeOfV < 100 || animateMillis < 16)
                return;

            if (animateMillis > maxDuration)
                animateMillis = maxDuration;

            var animations = new List<AnimationEntry>();

            var entry = new AnimationEntry(
                name: "Moving",
                start: (velocityX, velocityY),
                end: (0d, 0d),
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinIn,
                tick: FlingTick,
                final: FlingFinal
            );
            animations.Add(entry);

            Animation.Start(animations, (long)animateMillis);

            _animations = animations;
        }

        /// <summary>
        /// Stop all running animations
        /// </summary>
        public void StopRunningAnimations()
        {
            Animation.Stop(_animations, false);
        }

        private bool CenterTick(AnimationEntry entry, double value)
        {
            var x = ((MReadOnlyPoint)entry.Start).X + (((MReadOnlyPoint)entry.End).X - ((MReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            var y = ((MReadOnlyPoint)entry.Start).Y + (((MReadOnlyPoint)entry.End).Y - ((MReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);

            if (_viewport.Center.X == x && _viewport.Center.Y == y)
                return false;

            _viewport.SetCenter(x, y);

            Navigated?.Invoke(this, ChangeType.Continuous);

            return true;
        }

        private bool CenterFinal(AnimationEntry entry)
        {
            _animations.Remove(entry);

            _viewport.SetCenter((MReadOnlyPoint)entry.End);

            Navigated?.Invoke(this, ChangeType.Discrete);

            return true;
        }

        private bool ResolutionTick(AnimationEntry entry, double value)
        {
            var r = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);

            if (_viewport.Resolution == r)
                return false;

            _viewport.SetResolution(r);

            Navigated?.Invoke(this, ChangeType.Continuous);

            return true;
        }

        private bool ResolutionFinal(AnimationEntry entry)
        {
            _animations.Remove(entry);

            _viewport.SetResolution((double)entry.End);

            Navigated?.Invoke(this, ChangeType.Discrete);

            return true;
        }

        private bool RotationTick(AnimationEntry entry, double value)
        {
            var r = (double)entry.Start + _rotationDelta * entry.Easing.Ease(value);

            if (_viewport.Rotation == r)
                return false;

            _viewport.SetRotation(r);

            Navigated?.Invoke(this, ChangeType.Continuous);

            return true;
        }

        private bool RotationFinal(AnimationEntry entry)
        {
            _animations.Remove(entry);

            _viewport.SetRotation((double)entry.End);

            Navigated?.Invoke(this, ChangeType.Discrete);

            return true;
        }

        private bool FlingTick(AnimationEntry entry, double value)
        {
            var timeAmount = 16 / 1000d; // 16 milliseconds 

            var (velocityX, velocityY) = ((double, double))entry.Start;

            var xMovement = velocityX * (1d - entry.Easing.Ease(value)) * timeAmount;
            var yMovement = velocityY * (1d - entry.Easing.Ease(value)) * timeAmount;

            if (xMovement.IsNanOrInfOrZero())
                xMovement = 0;
            if (yMovement.IsNanOrInfOrZero())
                yMovement = 0;

            if (xMovement == 0 && yMovement == 0)
                return false;

            var previous = _viewport.ScreenToWorld(0, 0);
            var current = _viewport.ScreenToWorld(xMovement, yMovement);

            var xDiff = current.X - previous.X;
            var yDiff = current.Y - previous.Y;

            var newX = _viewport.Center.X + xDiff;
            var newY = _viewport.Center.Y + yDiff;

            if (_viewport.Center.X == newX && _viewport.Center.Y == newY)
                return false;

            _viewport.SetCenter(newX, newY);

            Navigated?.Invoke(this, ChangeType.Continuous);

            return true;
        }

        private bool FlingFinal(AnimationEntry entry)
        {
            _animations.Remove(entry);

            _viewport.SetCenter(_viewport.Center.X, _viewport.Center.Y);

            Navigated?.Invoke(this, ChangeType.Discrete);

            return true;
        }

        public bool UpdateAnimations(long ticks)
        {
            if (_animations.Count == 0)
            {
                return false;
            }

            return Animation.Update(_animations, ticks, true);
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
