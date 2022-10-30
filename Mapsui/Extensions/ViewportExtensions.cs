using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Extensions
{
    public static class ViewportExtensions
    {
        public static bool HasSize(this IReadOnlyViewport viewport) => 
            !viewport.Width.IsNanOrInfOrZero() && !viewport.Height.IsNanOrInfOrZero();
    }
}
