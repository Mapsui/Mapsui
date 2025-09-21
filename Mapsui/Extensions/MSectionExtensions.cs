using Mapsui.Logging;

namespace Mapsui.Extensions;

public static class MSectionExtensions
{
    public static int MaxPixels { get; set; } = 256 * 256 * 256; // Rough estimate of what is too big for realistic scenarios. It is based on TileWidth * TileHeight * MaxTilesInOneRequest
    public static bool CheckIfAreaIsTooBig(this MSection section)
    {
        var pixelsPerUnit = 1 / section.Resolution;
        var totalPixels = (long)(section.Extent.GetArea() * pixelsPerUnit * pixelsPerUnit); // We are dealing with an area so need to square the pixelsPerUnit.

        if (totalPixels > MaxPixels)
        {
            var message =
                $"The area requested is very large. " +
                $"The map section is '{totalPixels}' pixels in size. " +
                $"This may indicate a bug or configuration error. " +
                $"The '{nameof(MaxPixels)}' is '{MaxPixels}'. " +
                $"Change the '{nameof(MSectionExtensions)}.{nameof(MaxPixels)}' setting if the area is supposed to be this big. " +
                $"The resolution: '{section.Resolution:F2}'. " +
                $"The extent: '{section.Extent}'.";

            Logger.Log(LogLevel.Error, message);
            return true;
        }
        return false;
    }
}
