using Mapsui.Styles;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Extensions
{
    public static class StyleExtensions
    {
        public static bool ShouldBeApplied(this IStyle? style, double resolution)
        {
            if (style is null) return false;
            if (!style.Enabled) return false;
            if (style.MinVisible > resolution) return false;
            if (style.MaxVisible < resolution) return false;
            return true;
        }

        public static IEnumerable<IStyle> GetStylesToApply(this IStyle? style, double resolution)
        {
            if (style is null) return Enumerable.Empty<IStyle>();

            if (!style.ShouldBeApplied(resolution))
                return Enumerable.Empty<IStyle>();

            if (style is StyleCollection styleCollection)
            {
                return styleCollection.Styles.Where(s => s.ShouldBeApplied(resolution));
            }

            return new[] { style };
        }
    }
}
