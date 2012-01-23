// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

using System;
using System.Collections.Generic;
using SharpMap.Styles;
using SharpMap.Geometries;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Defines an axis-aligned box around a label, used for collision detection
    /// </summary>
    public class LabelBox : IComparable<LabelBox>
    {
        private double _height;
        private double _left;
        private double _top;
        private double _width;

        /// <summary>
        /// Initializes a new LabelBox instance
        /// </summary>
        /// <param name="left">Left side of box</param>
        /// <param name="top">Top of box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        public LabelBox(double left, double top, double width, double height)
        {
            _left = left;
            _top = top;
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Initializes a new LabelBox instance based on a rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        public LabelBox(BoundingBox rectangle)
        {
            _left = rectangle.MinX;
            _top = rectangle.MinY;
            _width = rectangle.Width;
            _height = rectangle.Height;
        }

        /// <summary>
        /// The Left tie-point for the Label
        /// </summary>
        public double Left
        {
            get { return _left; }
            set { _left = value; }
        }

        /// <summary>
        /// The Top tie-point for the label
        /// </summary>
        public double Top
        {
            get { return _top; }
            set { _top = value; }
        }

        /// <summary>
        /// Width of the box
        /// </summary>
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        /// Height of the box
        /// </summary>
        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        /// <summary>
        /// Right side of the box
        /// </summary>
        public double Right
        {
            get { return _left + _width; }
        }

        /// <summary>
        /// Bottom of th ebox
        /// </summary>
        public double Bottom
        {
            get { return _top - _height; }
        }

        #region IComparable<LabelBox> Members

        /// <summary>
        /// Returns 0 if the boxes intersects each other
        /// </summary>
        /// <param name="other">labelbox to perform intersectiontest with</param>
        /// <returns>0 if the intersect</returns>
        public int CompareTo(LabelBox other)
        {
            if (Intersects(other))
                return 0;
            else if (other.Left > Right ||
                     other.Bottom > Top)
                return 1;
            else
                return -1;
        }

        #endregion

        /// <summary>
        /// Determines whether the boundingbox intersects another boundingbox
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool Intersects(LabelBox box)
        {
            return !(box.Left > Right ||
                     box.Right < Left ||
                     box.Bottom > Top ||
                     box.Top < Bottom);
        }
    }

    /// <summary>
    /// Class for storing a label instance
    /// </summary>
    public class Label : IComparable<Label>, IComparer<Label>
    {
        private LabelBox _box;
        private Font _Font;
        private Point _LabelPoint;
        private int _Priority;
        private float _Rotation;
        private bool _show;
        private LabelStyle _Style;

        private string _Text;

        /// <summary>
        /// Initializes a new Label instance
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="labelpoint">Position of label</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="priority">Label priority used for collision detection</param>
        /// <param name="collisionbox">Box around label for collision detection</param>
        /// <param name="style">The style of the label</param>
        public Label(string text, Point labelpoint, float rotation, int priority, LabelBox collisionbox,
                     LabelStyle style)
        {
            _Text = text;
            _LabelPoint = labelpoint;
            _Rotation = rotation;
            _Priority = priority;
            _box = collisionbox;
            _Style = style;
            _show = true;
        }

        /// <summary>
        /// Show this label or don't
        /// </summary>
        public bool Show
        {
            get { return _show; }
            set { _show = value; }
        }

        /// <summary>
        /// The text of the label
        /// </summary>
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        /// <summary>
        /// Label position
        /// </summary>
        public Point LabelPoint
        {
            get { return _LabelPoint; }
            set { _LabelPoint = value; }
        }

        /// <summary>
        /// Label font
        /// </summary>
        public Font Font
        {
            get { return _Font; }
            set { _Font = value; }
        }

        /// <summary>
        /// Label rotation
        /// </summary>
        public float Rotation
        {
            get { return _Rotation; }
            set { _Rotation = value; }
        }

        /// <summary>
        /// Value indicating rendering priority
        /// </summary>
        public int Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        /// <summary>
        /// Label box
        /// </summary>
        public LabelBox Box
        {
            get { return _box; }
            set { _box = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SharpMap.Styles.LabelStyle"/> of this label
        /// </summary>
        public LabelStyle Style
        {
            get { return _Style; }
            set { _Style = value; }
        }

        #region IComparable<Label> Members

        /// <summary>
        /// Tests if two label boxes intersects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Label other)
        {
            if (this == other)
                return 0;
            else if (_box == null)
                return -1;
            else if (other.Box == null)
                return 1;
            else
                return _box.CompareTo(other.Box);
        }

        #endregion

        #region IComparer<Label> Members

        /// <summary>
        /// Checks if two labels intersect
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(Label x, Label y)
        {
            return x.CompareTo(y);
        }

        #endregion
    }
}