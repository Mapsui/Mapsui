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
        private double centerX;
        private double centerY;
        BoundingBox extent;
        private double height;
        private double resolution;
        private double width;

        public Viewport() { }

        public Viewport(Viewport viewport)
        {
            resolution = viewport.resolution;
            centerX = viewport.centerX;
            centerY = viewport.centerY;
            width = viewport.width;
            height = viewport.height;
            UpdateExtent();
        }

        public Point Center
        {
            set
            {
                centerX = value.X;
                centerY = value.Y;
                UpdateExtent();
            }
        }

        public double Resolution
        {
            get { return resolution; }
            set
            {
                resolution = value;
                UpdateExtent();
            }
        }

        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                UpdateExtent();
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                UpdateExtent();
            }
        }

        public double CenterX
        {
            get { return centerX; }
            set
            {
                centerX = value;
                UpdateExtent();
            }
        }

        public double CenterY
        {
            get { return centerY; }
            set
            {
                centerY = value;
                UpdateExtent();
            }
        }

        public BoundingBox Extent
        {
            get { return extent ?? (extent = new BoundingBox(0, 0, 0, 0)); }
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
            return new Point((worldX - extent.MinX) / resolution, (extent.MaxY - worldY) / resolution);
        }

        public Point ScreenToWorld(double screenX, double screenY)
        {
            return new Point((extent.MinX + screenX * resolution), (extent.MaxY - (screenY * resolution)));
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
            double spanX = width * resolution;
            double spanY = height * resolution;
            extent = new BoundingBox(
                CenterX - spanX * 0.5, CenterY - spanY * 0.5,
                CenterX + spanX * 0.5, CenterY + spanY * 0.5);
        }
    }
}
