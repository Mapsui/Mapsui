// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

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
        public delegate IStyle? GetStyleMethod(IFeature feature);

        /// <summary>
        /// Gets or sets the default style when an attribute isn't found in any bucket
        /// </summary>
        public IStyle? DefaultStyle { get; set; }

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

        public IStyle? GetStyle(IFeature row)
        {
            return StyleDelegate(row) ?? DefaultStyle;
        }
    }
}
