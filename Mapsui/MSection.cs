using System;

namespace Mapsui;

/// <summary>
/// MSection represents a part of the map defined by the extent and the resolution to indicate the level of detail.
/// It's main purpose is to define which data should be fetched.
/// </summary>
public record MSection
{
    public MSection(MRect extent, double resolution)
    {

        Extent = extent ?? throw new ArgumentNullException(nameof(extent));
        Resolution = resolution;
    }

    public MRect Extent { get; }
    public double Resolution { get; }
    public double ScreenWidth => Extent.Width / Resolution;
    public double ScreenHeight => Extent.Height / Resolution;
}
