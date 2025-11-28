using Mapsui.Styles;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Styles.Thematics;
using System;

namespace Mapsui.Extensions;

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

    /// <summary>
    /// Recursively resolves and flattens a style hierarchy into concrete renderable styles for a specific feature and viewport.
    /// Theme styles are evaluated (if visible) and their result is further flattened.
    /// Style collections are traversed depth-first preserving declaration order.
    /// Cycles lead to an <see cref="InvalidOperationException"/>.
    /// </summary>
    public static IEnumerable<IStyle> GetStylesToApply(this IStyle? style, IFeature feature, Viewport viewport)
    {
        var resolution = viewport.Resolution;
        var visited = new HashSet<IStyle>();
        return Flatten(style, feature, viewport, resolution, visited);

        static IEnumerable<IStyle> Flatten(IStyle? current, IFeature feature, Viewport viewport, double resolution, HashSet<IStyle> visited)
        {
            if (current is null) yield break;
            if (!current.ShouldBeApplied(resolution)) yield break;
            if (!visited.Add(current))
                throw new InvalidOperationException("Cycle detected in style graph while resolving styles.");

            switch (current)
            {
                case IThemeStyle themeStyle:
                    var resolved = themeStyle.GetStyle(feature, viewport);
                    foreach (var s in Flatten(resolved, feature, viewport, resolution, visited))
                        yield return s;
                    break;
                case StyleCollection sc:
                    foreach (var child in sc.Styles)
                        foreach (var s in Flatten(child, feature, viewport, resolution, visited))
                            yield return s;
                    break;
                default:
                    yield return current;
                    break;
            }
        }
    }
}
