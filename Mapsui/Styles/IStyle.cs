// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

namespace Mapsui.Styles
{
    /// <summary>
    /// Defines an interface for defining layer styles
    /// </summary>
    public interface IStyle
    {
        /// <summary>
        /// Gets or sets the minimum zoom value where the style is applied
        /// </summary>
        double MinVisible { get; set; }

        /// <summary>
        /// Gets or sets the maximum zoom value where the style is applied
        /// </summary>
        double MaxVisible { get; set; }

        /// <summary>
        /// Gets or sets whether objects in this style is rendered or not
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the objects overall opacity
        /// </summary>
        float Opacity { get; set; }
    }
}
