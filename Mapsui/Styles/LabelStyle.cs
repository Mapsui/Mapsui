// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using Mapsui.Providers;

namespace Mapsui.Styles
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
            Bottom = 2,
            /// <summary>
            /// Right oriented
            /// </summary>
            Top = 0,
            /// <summary>
            /// Centered
            /// </summary>
            Center = 1
        }

        public enum LineBreakMode : short
        {
            /// <summary>
            /// Do not wrap text
            /// </summary>
            NoWrap,
            /// <summary>
            /// Wrap at character boundaries
            /// </summary>
            CharacterWrap,
            /// <summary>
            /// Truncate the head of text
            /// </summary>
            HeadTruncation,
            /// <summary>
            /// Truncate the middle of text. This may be done, for example, by replacing it with an ellipsis
            /// </summary>
            MiddleTruncation,
            /// <summary>
            /// Truncate the tail of text
            /// </summary>
            TailTruncation,
            /// <summary>
            /// Wrap at word boundaries
            /// </summary>
            WordWrap
        }

        public LabelStyle()
        {
            Font = new Font { FontFamily = "Verdana", Size = 12 };
            Offset = new Offset { X = 0, Y = 0 };
            CollisionDetection = false;
            ForeColor = Color.Black;
            BackColor = new Brush { Color = Color.White };
            HorizontalAlignment = HorizontalAlignmentEnum.Center;
            VerticalAlignment = VerticalAlignmentEnum.Center;
            MaxWidth = 0;
            LineHeight = 1.0;
            WordWrap = LineBreakMode.NoWrap;
        }

        public LabelStyle(LabelStyle labelStyle)
        {
            Font = new Font(labelStyle.Font);
            Offset = new Offset(labelStyle.Offset);
            CollisionDetection = false;
            ForeColor = new Color(labelStyle.ForeColor);
            BackColor = new Brush(labelStyle.BackColor);
            HorizontalAlignment = HorizontalAlignmentEnum.Center;
            VerticalAlignment = VerticalAlignmentEnum.Center;
            MaxWidth = labelStyle.MaxWidth;
            WordWrap = labelStyle.WordWrap;
            LineHeight = labelStyle.LineHeight;
            Text = labelStyle.Text;
            LabelColumn = labelStyle.LabelColumn;
            LabelMethod = labelStyle.LabelMethod;
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
        public Brush BackColor { get; set; } // todo: rename

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
        /// The horisontal alignment of the text in relation to the labelpoint
        /// </summary>
        public HorizontalAlignmentEnum HorizontalAlignment { get; set; }

        /// <summary>
        /// The horisontal alignment of the text in relation to the labelpoint
        /// </summary>
        public VerticalAlignmentEnum VerticalAlignment { get; set; }

        /// <summary>
        /// Maximum width of text in em. If text is wider than this, text is shorten or 
        /// word wrapped regarding WordWrap.
        /// </summary>
        public double MaxWidth { get; set; }

        /// <summary>
        /// Line break mode for text, if width is bigger than MaxWidth
        /// </summary>
        public LineBreakMode WordWrap { get; set; }

        /// <summary>
        /// Space from one text line to next text line in em
        /// </summary>
        public double LineHeight { get; set; }

        /// <summary>The text used for this specific label.</summary>
        /// <remarks>Used only when LabelColumn and LabelMethod are not set.</remarks>
        public string Text { private get; set; }

        /// <summary>The column of the feature used by GetLabelText to return the label text.</summary>
        /// <remarks>Used only when LabelMethod is not set. Overrides use of the Text field.</remarks>
        public string LabelColumn { get; set; }

        /// <summary>Method used by GetLabelText to return the label text.</summary>
        /// <remarks>Overrides use of Text and LabelColumn fields.</remarks>
        public Func<IFeature, string> LabelMethod { get; set; }

        /// <summary>The text used for this specific label.</summary>
        public string GetLabelText(IFeature feature)
        {
            if (LabelMethod != null) return LabelMethod(feature);
            if (LabelColumn != null) return feature[LabelColumn].ToString();
            return Text;
        }


    }
}