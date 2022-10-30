using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Extensions
{
    public static class ViewportExtensions
    {
        /// <summary>
        /// True if Width and Height are not zero
        /// </summary>
        public static bool HasSize(this IReadOnlyViewport viewport) => 
            !viewport.Width.IsNanOrInfOrZero() && !viewport.Height.IsNanOrInfOrZero();

        /// <summary>
        /// IsRotated is true, when viewport displays map rotated
        /// </summary>
        public static bool IsRotated(this IReadOnlyViewport viewport) => 
            !double.IsNaN(viewport.Rotation) && viewport.Rotation > Constants.Epsilon 
            && viewport.Rotation < 360 - Constants.Epsilon;
    }
}
