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
    public class Viewport : IViewport
    {
        public event PropertyChangedEventHandler ViewportChanged;
        public bool Initialized { get; private set; }

        private readonly BoundingBox _extent;
        private Quad _windowExtent;
        private double _height;
        private double _resolution;
        private double _width;
        private double _rotation;
        private readonly NotifyingPoint _center = new NotifyingPoint();
        private bool _modified = true;
        

        public Viewport()
        {
            _extent = new BoundingBox(0, 0, 0, 0);
            _windowExtent = new Quad();
            // ReSharper disable once ExplicitCallerInfoArgument
            // In this case we don't want to caller to be passed (the Viewport constructor) but just the Center
			_center.PropertyChanged += (sender, args) => OnViewportChanged(nameof(Center));
        }
        
        public Viewport(Viewport viewport) : this()
        {
            _resolution = viewport._resolution;
            _width = viewport._width;
            _height = viewport._height;
            _rotation = viewport._rotation;
            _center.X = viewport._center.X;
            _center.Y = viewport._center.Y;
            if (viewport.Extent!= null) _extent = new BoundingBox(viewport.Extent);
            if (viewport.WindowExtent != null) _windowExtent = new Quad(
                viewport.WindowExtent.BottomLeft, viewport.WindowExtent.TopLeft,
                viewport.WindowExtent.TopRight, viewport.WindowExtent.BottomRight);
        }

        private void OnViewportChanged([CallerMemberName] string propertyName = null)
        {
            _modified = true;
            ViewportChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Point Center
        {
            get => _center;
            set
            {
                _center.X = value.X;
                _center.Y = value.Y;
                OnViewportChanged();
            }
        }

        public double Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnViewportChanged();
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                OnViewportChanged();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnViewportChanged();
            }
        }

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

        public bool IsRotated => 
            !double.IsNaN(_rotation) && _rotation > Constants.Epsilon && _rotation < 360 - Constants.Epsilon;

        public BoundingBox Extent
        {
            get
            {
                if (_modified) UpdateExtent(); 
                return _extent;
            }
        }

        public Quad WindowExtent
        {
            get
            {
                if (_modified) UpdateExtent();
                return _windowExtent;
            }
        }

        public Point WorldToScreen(Point worldPosition)
        {
            return WorldToScreen(worldPosition.X, worldPosition.Y);
        }

        public Point WorldToScreenUnrotated(Point worldPosition)
        {
            return WorldToScreenUnrotated(worldPosition.X, worldPosition.Y);
        }

        public Point ScreenToWorld(Point screenPosition)
        {
            return ScreenToWorld(screenPosition.X, screenPosition.Y);
        }

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

        public Point WorldToScreenUnrotated(double worldX, double worldY)
        {
            var screenCenterX = Width / 2.0;
            var screenCenterY = Height / 2.0;
            var screenX = (worldX - Center.X) / _resolution + screenCenterX;
            var screenY = (Center.Y - worldY) / _resolution + screenCenterY;

            return new Point(screenX, screenY);
        }

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

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, double deltaScale = 1, double deltaRotation = 0)
        {
            var previous = ScreenToWorld(previousScreenX, previousScreenY);
            var current = ScreenToWorld(screenX, screenY);

            var newX = _center.X + previous.X - current.X;
            var newY = _center.Y + previous.Y - current.Y;

            if (deltaScale != 1)
            {
                Resolution = Resolution / deltaScale;

                current = ScreenToWorld(screenX, screenY); // calculate current position again with adjusted resolution
                                                           // Zooming should be centered on the place where the map is touched. This is done with the scale correction.
                var scaleCorrectionX = (1 - deltaScale) * (current.X - Center.X);
                var scaleCorrectionY = (1 - deltaScale) * (current.Y - Center.Y);

                newX -= scaleCorrectionX;
                newY -= scaleCorrectionY;
            }           
            _center.X = newX;
            _center.Y = newY;

            if (deltaRotation != 0)
            {
                current = ScreenToWorld(screenX, screenY); // calculate current position again with adjusted resolution
                Rotation += deltaRotation;
                var postRotation = ScreenToWorld(screenX, screenY); // calculate current position again with adjusted resolution

                _center.X -= postRotation.X - current.X;
                _center.Y -= postRotation.Y - current.Y;
            }
        }

        private void UpdateExtent()
        {
            if (double.IsNaN(_center.X)) return;
            if (double.IsNaN(_center.Y)) return;
            if (double.IsNaN(_resolution)) return;

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

        public bool TryInitializeViewport(Map map, double screenWidth, double screenHeight)
        {
            if (screenWidth.IsNanOrZero()) return false;
            if (screenHeight.IsNanOrZero()) return false;

            if (double.IsNaN(map.Viewport.Resolution)) // only when not set yet
            {
                if (!map.Envelope.IsInitialized()) return false;
                if (map.Envelope.GetCentroid() == null) return false;

                if (Math.Abs(map.Envelope.Width) > Constants.Epsilon)
                    map.Viewport.Resolution = map.Envelope.Width / screenWidth;
                else
                    // An envelope width of zero can happen when there is no data in the Maps' layers (yet).
                    // It should be possible to start with an empty map.
                    map.Viewport.Resolution = Constants.DefaultResolution;
            }

            if (double.IsNaN(map.Viewport.Center.X) || double.IsNaN(map.Viewport.Center.Y)) // only when not set yet
            {
                if (!map.Envelope.IsInitialized()) return false;
                if (map.Envelope.GetCentroid() == null) return false;

                map.Viewport.Center = map.Envelope.GetCentroid();
            }

            map.Viewport.Width = screenWidth;
            map.Viewport.Height = screenHeight;
            
            Initialized = true;
            return true;
        }
    }
}
