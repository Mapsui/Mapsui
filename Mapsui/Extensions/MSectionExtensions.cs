using Mapsui.Logging;

namespace Mapsui.Extensions;

public static class MSectionExtensions
{
    public static int MaxMegaPixels { get; set; } = 100_000;
    public static bool CheckIfAreaIsTooBig(this MSection section)
    {
        var pixelsPerUnit = 1 / section.Resolution;
        var totalMegaPixels = section.Extent.GetArea() * pixelsPerUnit / 1_000_000;
        if (totalMegaPixels > MaxMegaPixels)
        {
            var message =
                $"The area requested is very large. " +
                $"The map section is '{totalMegaPixels:F2}' mega pixels in size." +
                $"This may indicate a bug or configuration error. " +
                $"The '{nameof(MaxMegaPixels)}' is '{MaxMegaPixels}'." +
                $"Change the '{nameof(MSectionExtensions)}.{nameof(MaxMegaPixels)}' setting if the area is supposed to be this big." +
                $"The resolution: '{section.Resolution:F2}'. " +
                $"The extent: '{section.Extent}'.";

            Logger.Log(LogLevel.Error, message);
            return true;
        }
        return false;
    }
}
