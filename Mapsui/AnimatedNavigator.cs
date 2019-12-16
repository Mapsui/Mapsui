using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public class AnimatedNavigator : INavigator
    {
        private readonly Map _map;
        private readonly IViewport _viewport;
        private Animation _animation;
        private double _rotationDelta;

        public EventHandler Navigated { get; set; } 

        public AnimatedNavigator(Map map, IViewport viewport)
        {
            _map = map;
            _viewport = viewport;
        }


        /// <inheritdoc />
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            NavigateTo(extent, scaleMethod, 300);
        }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determine resolution</param>
        /// <param name="duration">Duration of animation in millisecondsScale method to use to determine resolution</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit, long duration = 300)
        {
            if (extent == null) return;

            var resolution = ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, _viewport.Width, _viewport.Height, scaleMethod);

            NavigateTo(extent.Centroid, resolution, duration);
        }

        /// <inheritdoc />
        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill)
        {
            NavigateTo(_map.Envelope, scaleMethod, 300);
        }

        /// <summary>
        /// Navigate to a resolution, so such the map uses the fill method
        /// </summary>
        /// <param name="scaleMethod"></param>
        /// <param name="duration">Duration of animation in millisecondsScale method to use to determine resolution</param>
        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = 300)
        {
            NavigateTo(_map.Envelope, scaleMethod, duration);
        }

        /// <inheritdoc />
        public void NavigateTo(Point center, double resolution)
        {
            NavigateTo(center, resolution, 300);
        }

        /// <summary>
        /// Navigate to center and change resolution with animation
        /// </summary>
        /// <param name="center">New center to move to</param>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        public void NavigateTo(Point center, double resolution, long duration = 300)
        {
            // Stop any old animation if there is one
            if (_animation != null)
            {
                _animation.Stop(false);
                _animation = null;
            }

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

        /// <inheritdoc />
        public void ZoomTo(double resolution)
        {
            ZoomTo(resolution, 300);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        public void ZoomTo(double resolution, long duration = 300)
        {
            // Stop any old animation if there is one
            if (_animation != null)
            {
                _animation.Stop(false);
                _animation = null;
            }

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
        }

        /// <summary>
        /// Zoom out to a given point
        /// </summary>
        /// <param name="centerOfZoom">Center to use for zoom out</param>
        public void ZoomOut(Point centerOfZoom)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
            ZoomTo(resolution, centerOfZoom);
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
            CenterOn(x, y, 300);
        }

        /// <inheritdoc />
        public void CenterOn(Point center)
        {
            CenterOn(center.X, center.Y, 300);
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(double x, double y, long duration = 300)
        {
            CenterOn(new Point(x, y), duration);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        /// <param name="duration">Duration of animation in milliseconds</param>
        /// <param name="easing">Function for easing</param>
        public void CenterOn(Point center, long duration = 300)
        {
            // Stop any old animation if there is one
            if (_animation != null)
            {
                _animation.Stop(false);
                _animation = null;
            }

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
                    easing: Easing.Linear,
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
        /// <param name="duration">Duration for animation in milliseconds</param>
        public void FlyTo(Point center, double maxResolution, long duration = 2000)
        {
            // Stop any old animation if there is one
            if (_animation != null)
            {
                _animation.Stop(false);
                _animation = null;
            }

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

        /// <inheritdoc />
        public void RotateTo(double rotation)
        {
            RotateTo(rotation, 300);
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        public void RotateTo(double rotation, long duration)
        {
            // Stop any old animation if there is one
            if (_animation != null)
            {
                _animation.Stop(false);
                _animation = null;
            }

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
            var r = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);

            // Set new values
            _viewport.SetResolution(r);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        private void ResolutionFinal(AnimationEntry entry)
        {
            _viewport.SetResolution((double)entry.End);

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
    }
}
