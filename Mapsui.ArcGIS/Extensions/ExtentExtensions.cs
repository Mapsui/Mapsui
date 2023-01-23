using Mapsui.ArcGIS;

namespace Mapsui.ArcGIS.Extensions;

public static class ExtentExtensions
{
    public static MRect? ToMRect(this Extent? extent)
    {
        return extent == null ? null : new MRect(extent.xmin, extent.ymin, extent.xmax, extent.ymax);
    }
}
