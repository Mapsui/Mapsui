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

namespace SharpMap.Styles
{
    /// <summary>
    /// Defines a style used for rendering labels
    /// </summary>
    public class LabelStyle : Style
    {
        /// <summary>
        /// Label text alignment
        /// </summary>
        public enum HorizontalAlignmentEnum : short
        {
            /// <summary>
            /// Left oriented
            /// </summary>
            Left = 0,
            /// <summary>
            /// Right oriented
            /// </summary>
            Right = 2,
            /// <summary>
            /// Centered
            /// </summary>
            Center = 1
        }

        /// <summary>
        /// Label text alignment
        /// </summary>
        public enum VerticalAlignmentEnum : short
        {
            /// <summary>
            /// Left oriented
            /// </summary>
            Bottom = 0,
            /// <summary>
            /// Right oriented
            /// </summary>
            Top = 2,
            /// <summary>
            /// Centered
            /// </summary>
            Center = 1
        }

        /// <summary>
        /// Initializes a new LabelStyle
        /// </summary>
        public LabelStyle()
        {
            Font = new Font { FontFamily = "Times New Roman", Size = 12 };
            Offset = new Offset { X = 0, Y = 0 };
            CollisionDetection = false;
            CollisionBuffer = new Size { Width = 0, Height = 0 };
            ForeColor = Color.Black;
            HorizontalAlignment = HorizontalAlignmentEnum.Center;
            VerticalAlignment = VerticalAlignmentEnum.Center;
        }

        /// <summary>
        /// Label Font
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Font color
        /// </summary>
        public Color ForeColor { get; set; }

        /// <summary>
        /// The background color of the label. Set to transparent brush or null if background isn't needed
        /// </summary>
        public Brush BackColor { get; set; }

        /// <summary>
        /// Creates a halo around the text
        /// </summary>
        public Pen Halo { get; set; }

        /// <summary>
        /// Specifies relative position of labels with respect to objects label point
        /// </summary>
        public Offset Offset { get; set; }

        /// <summary>
        /// Gets or sets whether Collision Detection is enabled for the labels.
        /// If set to true, label collision will be tested.
        /// </summary>
        public bool CollisionDetection { get; set; }

        /// <summary>
        /// Distance around label where collision buffer is active
        /// </summary>
        public Size CollisionBuffer { get; set; }

        /// <summary>
        /// The horisontal alignment of the text in relation to the labelpoint
        /// </summary>
        public HorizontalAlignmentEnum HorizontalAlignment { get; set; }

        /// <summary>
        /// The horisontal alignment of the text in relation to the labelpoint
        /// </summary>
        public VerticalAlignmentEnum VerticalAlignment { get; set; }
    }
}