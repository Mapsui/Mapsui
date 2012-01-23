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
using System.Text;

namespace SharpMap.Styles
{
    /// <summary>
    /// Defines a style used for rendering vector data
    /// </summary>
    public class Style : IStyle
    {
        #region Privates

        #endregion

        /// <summary>
        /// Initializes a new Style and sets the default values
        /// </summary>
        /// <remarks>
        /// Default style values when initialized:<br/>
        /// *LineStyle: 1px solid black<br/>
        /// *FillStyle: Solid black<br/>
        /// *Outline: No Outline
        /// *Symbol: null-reference
        /// </remarks>
        public Style()
        {
            MinVisible = 0;
            MaxVisible = double.MaxValue;
            Enabled = true;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the minimum zoom value where the style is applied
        /// </summary>
        public double MinVisible { get; set; }

        /// <summary>
        /// Gets or sets the maximum zoom value where the style is applied
        /// </summary>
        public double MaxVisible { get; set; }

        /// <summary>
        /// Gets or sets whether objects in this style is rendered or not
        /// </summary>
        public bool Enabled { get; set; }

        #endregion
    }
}
