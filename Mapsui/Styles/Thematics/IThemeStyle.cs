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

using SharpMap.Providers;

namespace SharpMap.Styles.Thematics
{
	/// <summary>
	/// Interface for rendering a thematic layer
	/// </summary>
	public interface IThemeStyle : IStyle
	{
		/// <summary>
		/// Returns the style based on a feature
		/// </summary>
		/// <param name="attribute">Attribute to calculate color from</param>
		/// <returns>Color</returns>
		IStyle GetStyle(IFeature attribute);
	}
}
