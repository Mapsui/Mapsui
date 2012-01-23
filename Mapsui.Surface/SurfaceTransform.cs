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

using System.Windows;
using System.Windows.Media;
using BruTile;
using Mapsui.Windows;
using SharpMap;

namespace Mapsui.Surface
{
    class SurfaceTransform : IView
    {
        double resolution; //number of worldunits (meters or degrees) per screen unit (usually pixel, could be inch)
        Point center;
        double width;
        double height;
        SharpMap.Geometries.BoundingBox extent;
        MatrixTransform transform = new MatrixTransform();

        public SurfaceTransform()
        {
            transform.Matrix = new Matrix();
        }

        public double Resolution
        {
            get
            {
                return resolution;
            }
            set
            {
                resolution = value;
                UpdateExtent(resolution, center, width, height);
            }
        }

        public Point Center
        {
            set
            {
                center = value;
                UpdateExtent(resolution, center, width, height);
            }
        }

        public double Width
        {
            set
            {
                width = value;
                UpdateExtent(resolution, center, width, height);
            }
            get { return width; }
        }

        public double Height
        {
            set
            {
                height = value;
                UpdateExtent(resolution, center, width, height);
            }
            get { return height; }
        }

        public void Pan(Point currentMap, Point previousMap)
        {
            Point current = ViewToWorld(currentMap.X, currentMap.Y);
            Point previous = ViewToWorld(previousMap.X, previousMap.Y);
            Vector diff = Point.Subtract(previous, current);
            Center = Point.Add(center, diff);
        }

        public void Pan(Vector translate)
        {
            Vector vector = new Vector(-translate.X * resolution, translate.Y * resolution);
            center = Point.Add(center, vector);
            UpdateExtent(resolution, center, width, height);
        }

        public SharpMap.Geometries.BoundingBox Extent
        {
            get { return extent; }
        }

        private void UpdateExtent(double resolution, Point center, double width, double height)
        {
            if ((width == 0) || (height == 0)) return;

            double spanX = width * resolution;
            double spanY = height * resolution;
            extent = new SharpMap.Geometries.BoundingBox(center.X - spanX * 0.5, center.Y - spanY * 0.5, center.X + spanX * 0.5, center.Y + spanY * 0.5);

            Matrix matrix = ToMatrix(width, height, resolution, center);

            transform.Matrix = matrix;
        }

        private static Matrix ToMatrix(double width, double height, double resolution, Point center)
        {
            Matrix matrix = new Matrix();
            double mapCenterX = width * 0.5;
            double mapCenterY = height * 0.5;

            matrix.Translate(mapCenterX - center.X, mapCenterY - center.Y);

            matrix.ScaleAt(1 / resolution, 1 / resolution, mapCenterX, mapCenterY);

            matrix.Append(new Matrix(1, 0, 0, -1, 0, 0));
            matrix.Translate(0, height);
            return matrix;
        }

        public void Zoom(Rect zoomRect, Rect prevZoomRect)
        {
            Matrix matrix = transform.Matrix;
            matrix.Translate(-GetCenterX(prevZoomRect), -GetCenterY(prevZoomRect));
            double scale = zoomRect.Width / prevZoomRect.Width;
            matrix.Scale(scale, scale);
            matrix.Translate(GetCenterX(zoomRect), GetCenterY(zoomRect));
            transform.Matrix = matrix;
        }

        public void ScaleAt(double scale, Point origin)
        {
            Matrix matrix = transform.Matrix;
            matrix.ScaleAt(scale, scale, origin.X, origin.Y);
            transform.Matrix = matrix;
            if (transform.Inverse == null) return; //happens when extermely zoomed out.
            center = transform.Inverse.Transform(new Point(this.width / 2, this.height / 2));
            resolution = resolution / scale;
            UpdateExtent(this.resolution, this.center, this.width, this.height);
        }


        public Point WorldToView(double x, double y)
        {
            Point point;
            point = transform.Transform(new Point(x, y));
            return point;
        }

        public Point ViewToWorld(double x, double y)
        {
            Point point;
            GeneralTransform inverseTransform = transform.Inverse;

            point = inverseTransform.Transform(new Point(x, y));
            return point;
        }

        SharpMap.Geometries.Point IViewTransform.WorldToView(SharpMap.Geometries.Point point)
        {
            return ((IViewTransform)this).WorldToView(point.X, point.Y);
        }

        SharpMap.Geometries.Point IViewTransform.ViewToWorld(SharpMap.Geometries.Point point)
        {
            return ((IViewTransform)this).ViewToWorld(point.X, point.Y);
        }

        SharpMap.Geometries.Point IViewTransform.WorldToView(double x, double y)
        {
            Point point = WorldToView(x, y);
            return new SharpMap.Geometries.Point((float)point.X, (float)point.Y);
        }

        SharpMap.Geometries.Point IViewTransform.ViewToWorld(double x, double y)
        {
            Point point = ViewToWorld(x, y);
            return new SharpMap.Geometries.Point((float)point.X, (float)point.Y);
        }

        public Vector MapToWorld(Vector vector)
        {
            Point point;
            GeneralTransform inverseTransform = transform.Inverse;
            point = inverseTransform.Transform(new Point(vector.X - center.X, vector.Y - center.Y));
            return new Vector(point.X, point.Y);
        }

        private static double GetCenterX(Rect rect)
        {
            return ((rect.Left + rect.Right) * 0.5F);
        }

        private static double GetCenterY(Rect rect)
        {
            return ((rect.Top + rect.Bottom) * 0.5F);
        }
    }
}
