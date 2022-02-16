// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

namespace Mapsui.Styles.Thematics
{
    /// <summary>
    /// Interface for rendering a thematic layer
    /// </summary>
    public interface IThemeStyle : IStyle
    {
        /// <summary>
        /// Returns the style based on a feature
        /// </summary>
        /// <param name="feature">Feature to calculate color from</param>
        /// <returns>Color</returns>
        IStyle? GetStyle(IFeature feature);
    }
}
