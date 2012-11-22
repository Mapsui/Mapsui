// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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

using SharpMap.Geometries;

namespace SharpMap
{
    public class View : IView
    {
        #region Fields

        private double resolution;
        private double centerX;
        private double centerY;
        private double width;
        private double height;
        BoundingBox extent;   

        #endregion

        #region Public Methods

        public double Resolution
        {
            get { return resolution; }
            set
            {
                resolution = value;
                UpdateExtent();
            }
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
                UpdateExtent();
                centerX = value;
            }
        }

        public double CenterY
        {
            get { return centerY; }
            set
            {
                UpdateExtent();
                centerY = value;
            }
        }
        
        public BoundingBox Extent
        {
            get { return extent ?? (extent = new BoundingBox(0, 0, 0, 0)); }
        }

        public View() {}

        public View(View view)
        {
            resolution = view.resolution;
            centerX = view.centerX;
            centerY = view.centerY;
            width = view.width;
            height = view.height;
            UpdateExtent();
        }

        public Point WorldToView(Point point)
        {
            return WorldToView(point.X, point.Y);
        }

        public Point ViewToWorld(Point point)
        {
            return ViewToWorld(point.X, point.Y);
        }

        public Point WorldToView(double x, double y)
        {
            return new Point((x - extent.MinX) / resolution, (extent.MaxY - y) / resolution);
        }

        public Point ViewToWorld(double x, double y)
        {
            return new Point((extent.MinX + x * resolution), (extent.MaxY - (y * resolution)));
        }

        #endregion

        #region Private Methods

        private void UpdateExtent()
        {
            double spanX = width * resolution;
            double spanY = height * resolution;
            extent = new BoundingBox(
                CenterX - spanX * 0.5f, CenterY - spanY * 0.5f,
                CenterX + spanX * 0.5f, CenterY + spanY * 0.5f);
        }

        #endregion
    }
}
