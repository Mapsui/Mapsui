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

using System;
using Mapsui.Geometries;
using Mapsui.Utilities;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Mapsui
{
    /// <summary>
    /// Viewport holds all informations about the visible part of the map.
    /// </summary>
    /// <remarks>
    /// Viewport is the connection between Map and MapControl. It tells MapControl,
    /// which part of Map should be displayed on screen.
    /// </remarks>
    public class Viewport : IViewport
    {
        public event PropertyChangedEventHandler ViewportChanged;

        private readonly BoundingBox _extent;
        private Quad _windowExtent;
        private double _height;
        private double _resolution = Constants.DefaultResolution;
        private double _width;
        private double _rotation;
        private ReadOnlyPoint _center = new ReadOnlyPoint(0, 0);
        private bool _modified = true;
        /// <summary>
        /// Create a new viewport
        /// </summary>
        public Viewport()
        {
            _extent = new BoundingBox(0, 0, 0, 0);
            _windowExtent = new Quad();
        }

        /// <summary>
        /// Create a new viewport from another viewport
        /// </summary>
        /// <param name="viewport">Viewport from which to copy all values</param>
        public Viewport(Viewport viewport) : this()
        {
            _resolution = viewport._resolution;
            _width = viewport._width;
            _height = viewport._height;
            _rotation = viewport._rotation;
            _center = new ReadOnlyPoint(viewport._center);
            if (viewport.Extent != null) _extent = new BoundingBox(viewport.Extent);
            if (viewport.WindowExtent != null) _windowExtent = new Quad(viewport.WindowExtent);

            UpdateExtent();
        }

        public bool HasSize => !_width.IsNanOrInfOrZero() && !_height.IsNanOrInfOrZero();

        /// <inheritdoc />
        public ReadOnlyPoint Center
        {
            get => _center;
            set
            {
                // todo: Consider making setters private or removeing Set methods
                _center = value;
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
        public BoundingBox Extent
        {
            get
            {
                if (_modified) UpdateExtent();
                return _extent;
            }
        }

        /// <inheritdoc />
        public Quad WindowExtent
        {
            get
            {
                if (_modified) UpdateExtent();
                return _windowExtent;
            }
        }

        /// <inheritdoc />
        public Point WorldToScreen(Point worldPosition)
        {
            return WorldToScreen(worldPosition.X, worldPosition.Y);
        }

        /// <inheritdoc />
        public Point WorldToScreenUnrotated(Point worldPosition)
        {
            return WorldToScreenUnrotated(worldPosition.X, worldPosition.Y);
        }

        /// <inheritdoc />
        public Point ScreenToWorld(Point position)
        {
            return ScreenToWorld(position.X, position.Y);
        }

        /// <inheritdoc />
        public Point WorldToScreen(double worldX, double worldY)
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
        public Point WorldToScreenUnrotated(double worldX, double worldY)
        {
            var screenCenterX = Width / 2.0;
            var screenCenterY = Height / 2.0;
            var screenX = (worldX - Center.X) / _resolution + screenCenterX;
            var screenY = (Center.Y - worldY) / _resolution + screenCenterY;

            return new Point(screenX, screenY);
        }

        /// <inheritdoc />
        public Point ScreenToWorld(double screenX, double screenY)
        {
            var screenCenterX = Width / 2.0;
            var screenCenterY = Height / 2.0;

            if (IsRotated)
            {
                var screen = new Point(screenX, screenY).Rotate(_rotation, screenCenterX, screenCenterY);
                screenX = screen.X;
                screenY = screen.Y;
            }

            var worldX = Center.X + (screenX - screenCenterX) * _resolution;
            var worldY = Center.Y - (screenY - screenCenterY) * _resolution;
            return new Point(worldX, worldY);
        }

        /// <inheritdoc />
        public void Transform(Point positionScreen, Point previousPositionScreen, double deltaResolution = 1, double deltaRotation = 0)
        {
            var previous = ScreenToWorld(previousPositionScreen.X, previousPositionScreen.Y);
            var current = ScreenToWorld(positionScreen.X, positionScreen.Y);

            var newX = _center.X + previous.X - current.X;
            var newY = _center.Y + previous.Y - current.Y;

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

                SetCenter(_center.X - (postRotation.X - current.X), _center.Y - (postRotation.Y - current.Y));
            }
        }


        /// <summary>
        /// Recalculates extents for viewport
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
            _windowExtent.BottomLeft = new Point(left, bottom);
            _windowExtent.TopLeft = new Point(left, top);
            _windowExtent.TopRight = new Point(right, top);
            _windowExtent.BottomRight = new Point(right, bottom);

            if (!IsRotated)
            {
                _extent.Min.X = left;
                _extent.Min.Y = bottom;
                _extent.Max.X = right;
                _extent.Max.Y = top;
            }
            else
            {
                // Calculate the extent that will encompass a rotated viewport (slighly larger - used for tiles).
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

        public void SetCenter(double x, double y)
        {
            Center = new Point(x, y);
            OnViewportChanged();
        }

        public void SetCenter(ReadOnlyPoint center)
        {
            Center = center;
            OnViewportChanged();
        }

        public void SetResolution(double resolution)
        {
            Resolution = resolution;
            OnViewportChanged();
        }

        public void SetRotation(double rotation)
        {
            Rotation = rotation;
            OnViewportChanged();
        }

        /// <summary>
        /// Property change event
        /// </summary>
        /// <param name="propertyName">Name of property that changed</param>
        private void OnViewportChanged([CallerMemberName] string propertyName = null)
        {
            _modified = true;
            ViewportChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}
