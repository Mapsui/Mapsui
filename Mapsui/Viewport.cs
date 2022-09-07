// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Extensions;
using Mapsui.Utilities;
using Mapsui.ViewportAnimations;

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
        private double _rotation;
        private double _width;
        private double _height;

        // Derived from state
        private readonly MRect _extent;

        private List<AnimationEntry<Viewport>> _animations = new();

        /// <summary>
        /// Create a new viewport
        /// </summary>
        public Viewport()
        {
            _extent = new MRect(0, 0, 0, 0);
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

            IsRotated = viewport.IsRotated;
            if (viewport.Extent != null) _extent = new MRect(viewport.Extent);

            UpdateExtent();
        }

        public bool HasSize => !_width.IsNanOrInfOrZero() && !_height.IsNanOrInfOrZero();

        /// <inheritdoc />
        public MReadOnlyPoint Center => new MReadOnlyPoint(_centerX, _centerY);

        /// <inheritdoc />
        public double CenterX
        {
            get => _centerX;
            set
            {
                _centerX = value;
                UpdateExtent();
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
                UpdateExtent();
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
                UpdateExtent();
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
                UpdateExtent();
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
                UpdateExtent();
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

                IsRotated = !double.IsNaN(_rotation) && _rotation > Constants.Epsilon && _rotation < 360 - Constants.Epsilon;
                if (!IsRotated) _rotation = 0; // If not rotated set _rotation explicitly to exactly 0
                UpdateExtent();
                OnViewportChanged();
            }
        }

        /// <inheritdoc />
        public bool IsRotated { get; private set; }

        /// <inheritdoc />
        public MRect Extent => _extent;

        /// <inheritdoc />
        public MPoint WorldToScreen(MPoint worldPosition)
        {
            return WorldToScreen(worldPosition.X, worldPosition.Y);
        }

        /// <inheritdoc />
        public MPoint ScreenToWorld(MPoint position)
        {
            return ScreenToWorld(position.X, position.Y);
        }

        /// <inheritdoc />
        public MPoint ScreenToWorld(double positionX, double positionY)
        {
            var (x, y) = ScreenToWorldXY(positionX, positionY);
            return new MPoint(x, y);
        }

        /// <inheritdoc />
        public MPoint WorldToScreen(double worldX, double worldY)
        {
            var (x, y) = WorldToScreenXY(worldX, worldY);
            return new MPoint(x, y);
        }

        /// <inheritdoc />
        public (double screenX, double screenY) WorldToScreenXY(double worldX, double worldY)
        {
            var (screenX, screenY) = WorldToScreenUnrotated(worldX, worldY);

            if (IsRotated)
            {
                var screenCenterX = Width / 2.0;
                var screenCenterY = Height / 2.0;
                return Rotate(-_rotation, screenX, screenY, screenCenterX, screenCenterY);
            }

            return (screenX, screenY);
        }

        public (double x, double y) Rotate(double degrees, double x, double y, double centerX, double centerY)
        {
            // translate this point back to the center
            var newX = x - centerX;
            var newY = y - centerY;

            // rotate the values
            var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

            // translate back to original reference frame
            newX = p.X + centerX;
            newY = p.Y + centerY;

            return (newX, newY);
        }

        /// <inheritdoc />
        public (double screenX, double screenY) WorldToScreenUnrotated(double worldX, double worldY)
        {
            var screenCenterX = Width / 2.0;
            var screenCenterY = Height / 2.0;
            var screenX = (worldX - Center.X) / _resolution + screenCenterX;
            var screenY = (Center.Y - worldY) / _resolution + screenCenterY;
            return (screenX, screenY);
        }

        /// <inheritdoc />
        public (double worldX, double worldY) ScreenToWorldXY(double screenX, double screenY)
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
            return (worldX, worldY);
        }

        /// <inheritdoc />
        public void Transform(MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution = 1, double deltaRotation = 0)
        {
            _animations = new();

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

            CenterX = newX;
            CenterY = newY;

            if (deltaRotation != 0)
            {
                current = ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
                Rotation += deltaRotation;
                var postRotation = ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution

                CenterX = _centerX - (postRotation.X - current.X);
                CenterY = _centerY - (postRotation.Y - current.Y);
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
            var windowExtent = new MQuad
            {
                BottomLeft = new MPoint(left, bottom),
                TopLeft = new MPoint(left, top),
                TopRight = new MPoint(right, top),
                BottomRight = new MPoint(right, bottom)
            };

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
                windowExtent = windowExtent.Rotate(-_rotation, Center.X, Center.Y);
                var rotatedBoundingBox = windowExtent.ToBoundingBox();
                _extent.Min.X = rotatedBoundingBox.MinX;
                _extent.Min.Y = rotatedBoundingBox.MinY;
                _extent.Max.X = rotatedBoundingBox.MaxX;
                _extent.Max.Y = rotatedBoundingBox.MaxY;
            }
        }

        public void SetSize(double width, double height)
        {
            _animations = new();

            _width = width;
            _height = height;

            UpdateExtent();
            OnViewportChanged();
        }

        public void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
        {
            _animations = new();

            _centerX = x;
            _centerY = y;

            UpdateExtent();
            OnViewportChanged();
        }

        public void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
        {
            _animations = new();

            if (duration == 0)
            {
                _centerX = x;
                _centerY = y;
                _resolution = resolution;
            }
            else
            {
                _animations = ZoomOnCenterAnimation.Create(this, x, y, resolution, duration);
            }

            UpdateExtent();
            OnViewportChanged();
        }

        public void SetCenter(MReadOnlyPoint center, long duration = 0, Easing? easing = default)
        {
            _animations = new();

            if (center.Equals(Center))
                return;

            if (duration == 0)
            {
                _centerX = center.X;
                _centerY = center.Y;
            }
            else
            {
                _animations = CenterAnimation.Create(this, center.X, center.Y, duration, easing);
            }

            UpdateExtent();
            OnViewportChanged();
        }

        public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
        {
            _animations = new();

            if (Resolution == resolution)
                return;

            if (duration == 0)
                Resolution = resolution;
            else
            {
                _animations = ZoomAnimation.Create(this, resolution, duration, easing);
            }

            UpdateExtent();
            OnViewportChanged();
        }

        public void SetRotation(double rotation, long duration = 0, Easing? easing = default)
        {
            _animations = new();

            if (Rotation == rotation) return;

            if (duration == 0)
                Rotation = rotation;
            else
            {
                _animations = RotateAnimation.Create(this, rotation, duration, easing);
            }

            UpdateExtent();
            OnViewportChanged();
        }

        /// <summary>
        /// Property change event
        /// </summary>
        /// <param name="propertyName">Name of property that changed</param>
        private void OnViewportChanged([CallerMemberName] string? propertyName = null)
        {
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
            if (_animations.All(a => a.Done)) _animations = new List<AnimationEntry<Viewport>>();
            return Animation.UpdateAnimations(this, _animations);
        }

        public void SetAnimations(List<AnimationEntry<Viewport>> animations)
        {
            _animations = animations;
        }
    }
}
