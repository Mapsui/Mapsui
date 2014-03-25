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

using System;
using Mapsui.Geometries;

namespace Mapsui
{
    public class Viewport : IViewport
    {
        BoundingBox _extent;
        private double _height;
        private double _resolution;
        private double _width;
        private readonly NotifyingPoint _center = new NotifyingPoint();

        public Viewport()
        {
            RenderResolutionMultiplier = 1;
            _center.PropertyChanged += (sender, args) => UpdateExtent();
        }
        
        public Viewport(Viewport viewport) : this()
        {
            _resolution = viewport._resolution;
            _width = viewport._width;
            _height = viewport._height;
            RenderResolutionMultiplier = viewport.RenderResolutionMultiplier;
            UpdateExtent();
        }

        public double RenderResolutionMultiplier { get; set; }

        public double RenderResolution
        {
            get { return Resolution * RenderResolutionMultiplier; }
        }

        public Point Center
        {
            get { return _center; }
            set
            {
                _center.X = value.X;
                _center.Y = value.Y;
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

            var newX = Center.X + previous.X - current.X;
            var newY = Center.Y + previous.Y - current.Y;

            // When you pinch zoom outside the center of the map 
            // this will also affect the new center. 
            var scaleCorrectionX = (1 - deltaScale) * (current.X - Center.X);
            var scaleCorrectionY = (1 - deltaScale) * (current.Y - Center.Y);

            Resolution = Resolution / deltaScale;
            Center.X = newX - scaleCorrectionX;
            Center.Y = newY - scaleCorrectionY;
        }

        private void UpdateExtent()
        {
            var spanX = _width * _resolution;
            var spanY = _height * _resolution;
            _extent = new BoundingBox(
                Center.X - spanX * 0.5, Center.Y - spanY * 0.5,
                Center.X + spanX * 0.5, Center.Y + spanY * 0.5);
        }
    }
}
