// Copyright 2006 - Morten Nielsen (www.iter.dk)
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

using Mapsui.Providers;

namespace Mapsui.Styles.Thematics
{
	/// <summary>
	/// The CustomTheme class is used for defining your own thematic rendering by using a custom get-style-delegate.
	/// </summary>
	public class CustomTheme : Style, IThemeStyle
	{
		/// <summary>
		/// Custom Style Delegate method
		/// </summary>
		/// <param name="feature">Feature</param>
		/// <returns>Style to be applied to feature</returns>
		public delegate IStyle GetStyleMethod(IFeature feature);

	    /// <summary>
	    /// Gets or sets the default style when an attribute isn't found in any bucket
	    /// </summary>
	    public IStyle DefaultStyle { get; set; }

	    /// <summary>
	    /// Gets or sets the style delegate used for determining the style of a feature
	    /// </summary>
	    /// <seealso cref="GetStyleMethod"/>
	    public GetStyleMethod StyleDelegate { get; set; }


	    /// <summary>
		/// Initializes a new instance of the <see cref="CustomTheme"/> class
		/// </summary>
		/// <param name="getStyleMethod"></param>
		public CustomTheme(GetStyleMethod getStyleMethod)
		{
			StyleDelegate = getStyleMethod;
		}

		public IStyle GetStyle(IFeature row)
		{
            return StyleDelegate(row) ?? DefaultStyle;
		}
    }
}
