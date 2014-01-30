// Copyright 2012 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using Mapsui.Geometries;

namespace Mapsui
{
    public class Viewport : IViewport
    {
        private double _centerX;
        private double _centerY;
        BoundingBox _extent;
        private double _height;
        private double _resolution;
        private double _width;

        public Viewport()
        {
            RenderScaleFactor = 1;
        }

        public Viewport(Viewport viewport)
        {
            _resolution = viewport._resolution;
            _centerX = viewport._centerX;
            _centerY = viewport._centerY;
            _width = viewport._width;
            _height = viewport._height;
            UpdateExtent();
        }

        public double RenderScaleFactor { get; set; }

        public double RenderResolution
        {
            get { return Resolution * RenderScaleFactor; }
        }

        public Point Center
        {
            get { return new Point(_centerX, _centerY); }
            set
            {
                _centerX = value.X;
                _centerY = value.Y;
                UpdateExtent();
            }
        }

        public double Resolution
        {
            get { return _resolution; }
            set
            {
                _resolution = value;
                UpdateExtent();
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                UpdateExtent();
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                UpdateExtent();
            }
        }

        public double CenterX
        {
            get { return _centerX; }
            set
            {
                _centerX = value;
                UpdateExtent();
            }
        }

        public double CenterY
        {
            get { return _centerY; }
            set
            {
                _centerY = value;
                UpdateExtent();
            }
        }

        public BoundingBox Extent
        {
            get { return _extent ?? (_extent = new BoundingBox(0, 0, 0, 0)); }
        }

        public Point WorldToScreen(Point worldPosition)
        {
            return WorldToScreen(worldPosition.X, worldPosition.Y);
        }

        public Point ScreenToWorld(Point screenPosition)
        {
            return ScreenToWorld(screenPosition.X, screenPosition.Y);
        }

        public Point WorldToScreen(double worldX, double worldY)
        {
            return new Point((worldX - _extent.MinX) / _resolution, (_extent.MaxY - worldY) / _resolution);
        }

        public Point ScreenToWorld(double screenX, double screenY)
        {
            return new Point((_extent.MinX + screenX * _resolution), (_extent.MaxY - (screenY * _resolution)));
        }

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, double deltaScale = 1)
        {
            var previous = ScreenToWorld(previousScreenX, previousScreenY);
            var current = ScreenToWorld(screenX, screenY);

            var newX = CenterX + previous.X - current.X;
            var newY = CenterY + previous.Y - current.Y;

            // When you pinch zoom outside the center of the map 
            // this will also affect the new center. 
            var scaleCorrectionX = (1 - deltaScale) * (current.X - CenterX);
            var scaleCorrectionY = (1 - deltaScale) * (current.Y - CenterY);

            Resolution = Resolution / deltaScale;
            CenterX = newX - scaleCorrectionX;
            CenterY = newY - scaleCorrectionY;
        }

        private void UpdateExtent()
        {
            double spanX = _width * _resolution;
            double spanY = _height * _resolution;
            _extent = new BoundingBox(
                CenterX - spanX * 0.5, CenterY - spanY * 0.5,
                CenterX + spanX * 0.5, CenterY + spanY * 0.5);
        }
    }
}
