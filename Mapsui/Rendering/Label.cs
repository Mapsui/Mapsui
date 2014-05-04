// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Collections.Generic;
using Mapsui.Styles;
using Mapsui.Geometries;

namespace Mapsui.Rendering
{
    /// <summary>
    /// Defines an axis-aligned box around a label, used for collision detection
    /// </summary>
    public class LabelBox : IComparable<LabelBox>
    {
        /// <summary>
        /// Initializes a new LabelBox instance
        /// </summary>
        /// <param name="left">Left side of box</param>
        /// <param name="top">Top of box</param>
        /// <param name="width">Width of the box</param>
        /// <param name="height">Height of the box</param>
        public LabelBox(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new LabelBox instance based on a rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        public LabelBox(BoundingBox rectangle)
        {
            Left = rectangle.MinX;
            Top = rectangle.MinY;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        /// <summary>
        /// The Left tie-point for the Label
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// The Top tie-point for the label
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Width of the box
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the box
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Right side of the box
        /// </summary>
        public double Right
        {
            get { return Left + Width; }
        }

        /// <summary>
        /// Bottom of th ebox
        /// </summary>
        public double Bottom
        {
            get { return Top - Height; }
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
            if (other.Left > Right || other.Bottom > Top)
                return 1;
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
        /// <summary>
        /// Initializes a new Label instance
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="labelpoint">Position of label</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="priority">Label priority used for collision detection</param>
        /// <param name="collisionbox">Box around label for collision detection</param>
        /// <param name="style">The style of the label</param>
        public Label(string text, Point labelpoint, double rotation, int priority, LabelBox collisionbox,
                     LabelStyle style)
        {
            Text = text;
            LabelPoint = labelpoint;
            Rotation = rotation;
            Priority = priority;
            Box = collisionbox;
            Style = style;
            Show = true;
        }

        /// <summary>
        /// Show this label or don't
        /// </summary>
        public bool Show { get; set; }

        /// <summary>
        /// The text of the label
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Label position
        /// </summary>
        public Point LabelPoint { get; set; }

        /// <summary>
        /// Label font
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Label rotation
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// Value indicating rendering priority
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Label box
        /// </summary>
        public LabelBox Box { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Mapsui.Styles.LabelStyle"/> of this label
        /// </summary>
        public LabelStyle Style { get; set; }

        public Pen Halo { get; set; }

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
            if (Box == null)
                return -1;
            if (other.Box == null)
                return 1;
            return Box.CompareTo(other.Box);
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