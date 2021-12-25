// Copyright 2012 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    /// <summary>
    /// Viewport holds all information about the visible part of the map.
    /// </summary>
    /// <remarks>
    /// Viewport is the connection between Map and MapControl. It tells MapControl,
    /// which part of Map should be displayed on screen.
    /// </remarks>
    public class Viewport : IViewport
    {
        public event PropertyChangedEventHandler? ViewportChanged;

        // State
        private double _centerX;
        private double _centerY;
        private double _resolution = Constants.DefaultResolution;
        private double _width;
        private double _height;
        private double _rotation;

        // For derived state
        private bool _modified = true;
        private readonly MRect _extent;
        private MQuad _windowExtent;

        private List<AnimationEntry> _animations = new();

        /// <summary>
        /// Create a new viewport
        /// </summary>
        public Viewport()
        {
            _extent = new MRect(0, 0, 0, 0);
            _windowExtent = new MQuad();
        }

        /// <summary>
        /// Create a new viewport from another viewport
        /// </summary>
        /// <param name="viewport">Viewport from which to copy all values</param>
        public Viewport(IReadOnlyViewport viewport) : this()
        {
            _centerX = viewport.Center.X;
            _centerY = viewport.Center.Y;
            _resolution = viewport.Resolution;
            _width = viewport.Width;
            _height = viewport.Height;
            _rotation = viewport.Rotation;

            if (viewport.Extent != null) _extent = new MRect(viewport.Extent);
            if (viewport.WindowExtent != null) _windowExtent = new MQuad(viewport.WindowExtent);

            UpdateExtent();
        }

        public bool HasSize => !_width.IsNanOrInfOrZero() && !_height.IsNanOrInfOrZero();

        /// <inheritdoc />
        public MReadOnlyPoint Center
        {
            get => new MReadOnlyPoint(_centerX, _centerY);
        }

        /// <inheritdoc />
        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                OnViewportChanged();
            }
        }

        /// <inheritdoc />
        public double CenterY
        {
            get => _centerY;
            set
            {
                _centerY = value;
                OnViewportChanged();
            }
        }


        /// <inheritdoc />
        public double Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnViewportChanged();
            }
        }

        /// <inheritdoc />
        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                OnViewportChanged();
            }
        }

        /// <inheritdoc />
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnViewportChanged();
            }
        }

        /// <inheritdoc />
        public double Rotation
        {
            get => _rotation;
            set
            {
                // normalize the value to be [0, 360)
                _rotation = value % 360.0;
                if (_rotation < 0)
                    _rotation += 360.0;
                OnViewportChanged();
            }
        }

        /// <inheritdoc />
        public bool IsRotated =>
            !double.IsNaN(_rotation) && _rotation > Constants.Epsilon && _rotation < 360 - Constants.Epsilon;

        /// <inheritdoc />
        public MRect Extent
        {
            get
            {
                if (_modified) UpdateExtent();
                return _extent;
            }
        }

        /// <inheritdoc />
        public MQuad WindowExtent
        {
            get
            {
                if (_modified) UpdateExtent();
                return _windowExtent;
            }
        }

        /// <inheritdoc />
        public MPoint WorldToScreen(MPoint worldPosition)
        {
            return WorldToScreen(worldPosition.X, worldPosition.Y);
        }

        /// <inheritdoc />
        public MPoint WorldToScreenUnrotated(MPoint worldPosition)
        {
            return WorldToScreenUnrotated(worldPosition.X, worldPosition.Y);
        }

        /// <inheritdoc />
        public MPoint ScreenToWorld(MPoint position)
        {
            return ScreenToWorld(position.X, position.Y);
        }

        /// <inheritdoc />
        public MPoint WorldToScreen(double worldX, double worldY)
        {
            var p = WorldToScreenUnrotated(worldX, worldY);

            if (IsRotated)
            {
                var screenCenterX = Width / 2.0;
                var screenCenterY = Height / 2.0;
                p = p.Rotate(-_rotation, screenCenterX, screenCenterY);
            }

            return p;
        }

        /// <inheritdoc />
        public MPoint WorldToScreenUnrotated(double worldX, double worldY)
        {
            var screenCenterX = Width / 2.0;
            var screenCenterY = Height / 2.0;
            var screenX = (worldX - Center.X) / _resolution + screenCenterX;
            var screenY = (Center.Y - worldY) / _resolution + screenCenterY;

            return new MPoint(screenX, screenY);
        }

        /// <inheritdoc />
        public MPoint ScreenToWorld(double screenX, double screenY)
        {
            var screenCenterX = Width / 2.0;
            var screenCenterY = Height / 2.0;

            if (IsRotated)
            {
                var screen = new MPoint(screenX, screenY).Rotate(_rotation, screenCenterX, screenCenterY);
                screenX = screen.X;
                screenY = screen.Y;
            }

            var worldX = Center.X + (screenX - screenCenterX) * _resolution;
            var worldY = Center.Y - (screenY - screenCenterY) * _resolution;
            return new MPoint(worldX, worldY);
        }

        /// <inheritdoc />
        public void Transform(MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution = 1, double deltaRotation = 0)
        {
            var previous = ScreenToWorld(previousPositionScreen.X, previousPositionScreen.Y);
            var current = ScreenToWorld(positionScreen.X, positionScreen.Y);

            var newX = _centerX + previous.X - current.X;
            var newY = _centerY + previous.Y - current.Y;

            if (deltaResolution != 1)
            {
                Resolution = Resolution / deltaResolution;

                // Calculate current position again with adjusted resolution
                // Zooming should be centered on the place where the map is touched.
                // This is done with the scale correction.
                var scaleCorrectionX = (1 - deltaResolution) * (current.X - Center.X);
                var scaleCorrectionY = (1 - deltaResolution) * (current.Y - Center.Y);

                newX -= scaleCorrectionX;
                newY -= scaleCorrectionY;
            }

            SetCenter(newX, newY);

            if (deltaRotation != 0)
            {
                current = ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
                Rotation += deltaRotation;
                var postRotation = ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution

                SetCenter(_centerX - (postRotation.X - current.X), _centerY - (postRotation.Y - current.Y));
            }
        }


        /// <summary>
        /// Recalculates extent for viewport
        /// </summary>
        private void UpdateExtent()
        {
            // calculate the window extent which is not rotate
            var halfSpanX = _width * _resolution * 0.5;
            var halfSpanY = _height * _resolution * 0.5;
            var left = Center.X - halfSpanX;
            var bottom = Center.Y - halfSpanY;
            var right = Center.X + halfSpanX;
            var top = Center.Y + halfSpanY;
            _windowExtent.BottomLeft = new MPoint(left, bottom);
            _windowExtent.TopLeft = new MPoint(left, top);
            _windowExtent.TopRight = new MPoint(right, top);
            _windowExtent.BottomRight = new MPoint(right, bottom);

            if (!IsRotated)
            {
                _extent.Min.X = left;
                _extent.Min.Y = bottom;
                _extent.Max.X = right;
                _extent.Max.Y = top;
            }
            else
            {
                // Calculate the extent that will encompass a rotated viewport (slightly larger - used for tiles).
                // Perform rotations on corner offsets and then add them to the Center point.
                _windowExtent = _windowExtent.Rotate(-_rotation, Center.X, Center.Y);
                var rotatedBoundingBox = _windowExtent.ToBoundingBox();
                _extent.Min.X = rotatedBoundingBox.MinX;
                _extent.Min.Y = rotatedBoundingBox.MinY;
                _extent.Max.X = rotatedBoundingBox.MaxX;
                _extent.Max.Y = rotatedBoundingBox.MaxY;
            }

            _modified = false;
        }

        public void SetSize(double width, double height)
        {
            _width = width;
            _height = height;
            OnViewportChanged();
        }

        public void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
        {
            _centerX = x;
            _centerY = y;
            OnViewportChanged();
        }

        public void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
        {
            if (duration == 0)
            {
                _centerX = x;
                _centerY = y;
                Resolution = resolution;
            }
            else
            {
                _animations = CreateSetCenterAndResolutionsAnimation(x, y, resolution, easing);
                //!!!_animations = CreateZoomToAnimation(resolution, new MReadOnlyPoint(x, y), duration);
                Animation.Start(_animations, duration);
            }

            OnViewportChanged();
        }

        private List<AnimationEntry> CreateSetCenterAndResolutionsAnimation(double x, double y, double resolution, Easing? easing)
        {
            var animations = new List<AnimationEntry>();
            var entry = new AnimationEntry(
                start: Center,
                end: (MReadOnlyPoint)new MPoint(x, y),
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinInOut,
                tick: CenterTick,
                final: CenterFinal
            );
            animations.Add(entry);

            var entry2 = new AnimationEntry(
                 start: Resolution,
                 end: resolution,
                 animationStart: 0,
                 animationEnd: 1,
                 easing: easing ?? Easing.SinInOut,
                 tick: ResolutionTick,
                 final: ResolutionFinal
             );
            animations.Add(entry2);
            return animations;
        }

        private List<AnimationEntry> CreateZoomToAnimation(double resolution, MReadOnlyPoint centerOfZoom, long duration)
        {
            var animations = new List<AnimationEntry>();

            var entry = new AnimationEntry(
                start: (Center, Resolution),
                end: (centerOfZoom, resolution),
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.SinInOut,
                tick: CenterAndResolutionTick,
                final: CenterAndResolutionFinal
            );
            animations.Add(entry);

            Animation.Start(animations, duration);

            return animations;
        }

        private void CenterAndResolutionTick(AnimationEntry entry, double value)
        {
            var start = ((MReadOnlyPoint Center, double Resolution))entry.Start;
            var end = ((MReadOnlyPoint Center, double Resolution))entry.End;

            var x = start.Center.X + end.Center.X - (start.Center.X * entry.Easing.Ease(value));
            var y = start.Center.Y + end.Center.Y - (start.Center.Y * entry.Easing.Ease(value));
            var r = start.Resolution + end.Resolution - start.Resolution * entry.Easing.Ease(value);

            _centerX = x;
            _centerY = y;
            Resolution = r;
        }

        private void CenterAndResolutionFinal(AnimationEntry entry)
        {
            var end = ((MReadOnlyPoint Center, double Resolution))entry.End;
            _centerX = end.Center.X;
            _centerY = end.Center.Y;
            Resolution = end.Resolution;
        }

        public void SetCenter(MReadOnlyPoint center, long duration = 0, Easing? easing = default)
        {
            if (center.Equals(Center))
                return;

            if (duration == 0)
            {
                _centerX = center.X;
                _centerY = center.Y;
            }
            else
            {
                _animations = CreateSetCenterAnimation(center, easing);
                Animation.Start(_animations, duration);
            }
            OnViewportChanged();
        }

        private List<AnimationEntry> CreateSetCenterAnimation(MReadOnlyPoint center, Easing? easing)
        {
            return new List<AnimationEntry> { new AnimationEntry(
                                start: Center,
                                end: center,
                                animationStart: 0,
                                animationEnd: 1,
                                easing: easing ?? Easing.SinOut,
                                tick: CenterTick,
                                final: CenterFinal
                            ) };
        }

        private void CenterTick(AnimationEntry entry, double value)
        {
            _centerX = ((MReadOnlyPoint)entry.Start).X + (((MReadOnlyPoint)entry.End).X - ((MReadOnlyPoint)entry.Start).X) * entry.Easing.Ease(value);
            _centerY = ((MReadOnlyPoint)entry.Start).Y + (((MReadOnlyPoint)entry.End).Y - ((MReadOnlyPoint)entry.Start).Y) * entry.Easing.Ease(value);
        }

        private void CenterFinal(AnimationEntry entry)
        {
            _centerX = ((MReadOnlyPoint)entry.End).X;
            _centerY = ((MReadOnlyPoint)entry.End).Y;
        }

        private void ResolutionFinal(AnimationEntry entry)
        {
            Resolution = (double)entry.End;
        }

        private void ResolutionTick(AnimationEntry entry, double value)
        {
            Resolution = (double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value);
        }

        public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
        {
            if (Resolution == resolution)
                return;

            if (duration == 0)
                Resolution = resolution;
            else
            {
                _animations = CreateSetResolutionAnimation(resolution, Resolution, duration, easing);
            }

            OnViewportChanged();
        }

        private List<AnimationEntry> CreateSetResolutionAnimation(double resolution, double currentResolution, long duration, Easing? easing)
        {
            var animations = new List<AnimationEntry>();

            var entry = new AnimationEntry(
                start: currentResolution,
                end: resolution,
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinInOut,
                tick: ResolutionTick,
                final: ResolutionFinal
            );
            animations.Add(entry);

            Animation.Start(animations, duration);
            return animations;
        }

        public void SetRotation(double rotation, long duration = 0, Easing? easing = default)
        {
            if (Rotation == rotation) return;

            if (duration == 0)
                Rotation = rotation;
            else
            {
                var rotationDelta = rotation - Rotation;

                if (rotationDelta < -180.0)
                    rotationDelta += 360.0;

                if (rotationDelta > 180.0)
                    rotationDelta -= 360.0;

                var newRotation = Rotation + rotationDelta;

                _animations = CreateRotationAnimationEntry(newRotation, easing);
                Animation.Start(_animations, duration);

            }
            OnViewportChanged();
        }

        private List<AnimationEntry> CreateRotationAnimationEntry(double rotation, Easing? easing)
        {
            return new List<AnimationEntry> { new AnimationEntry(
                start: Rotation,
                end: rotation,
                animationStart: 0,
                animationEnd: 1,
                easing: easing ?? Easing.SinInOut,
                tick: (e, v) => Rotation = (double)e.Start + ((double)e.End * e.Easing.Ease(v)),
                final: (e) => Rotation = (double)e.End
            ) };
        }

        /// <summary>
        /// Property change event
        /// </summary>
        /// <param name="propertyName">Name of property that changed</param>
        private void OnViewportChanged([CallerMemberName] string? propertyName = null)
        {
            _modified = true;
            ViewportChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Viewport Create(MRect extent, double resolution)
        {
            return new Viewport
            {
                Resolution = resolution,
                _centerX = extent.Centroid.X,
                _centerY = extent.Centroid.Y,
                Width = extent.Width / resolution,
                Height = extent.Height / resolution
            };
        }

        public bool UpdateAnimations()
        {
            if (_animations.All(a => a.Done)) _animations.Clear();
            return Animation.UpdateAnimations(_animations);
        }

        public void SetAnimations(List<AnimationEntry> animations)
        {
            _animations = animations;
        }
    }
}
