using System;

namespace Mapsui;

public class Dimensions()
{
    // Width of the map service in pixels. Guaranteed to be greater than 0.
    public double Width { get; }
    // Height of the map service in pixels. Guaranteed to be greater than 0.
    public double Height { get; }
    // Pixel density of the map service. Guaranteed to be initialized.
    public float PixelDensity { get; }

    public Dimensions(double width, double height, float pixelDensity) : this()
    {
        if (double.IsNaN(width)) throw new ArgumentException("Width cannot be NaN", nameof(width));
        if (double.IsNaN(height)) throw new ArgumentException("Height cannot be NaN", nameof(height));
        if (float.IsNaN(pixelDensity)) throw new ArgumentException("Pixel density cannot be NaN", nameof(pixelDensity));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than 0.");
        if (pixelDensity <= 0) throw new ArgumentOutOfRangeException(nameof(pixelDensity), "Pixel density must be greater than 0.");

        Width = width;
        Height = height;
        PixelDensity = pixelDensity;
    }

    /// <summary>
    /// Converts coordinates in device independent units (or DIP or DP) to raw pixels.
    /// </summary>
    /// <param name="coordinateInDeviceIndependentUnits">Coordinate in device independent units (or DIP or DP)</param>
    /// <returns>Coordinate in raw pixels</returns>
    public MPoint ToCoordinateInRawPixels(MPoint coordinateInDeviceIndependentUnits)
    {
        return new MPoint(
            coordinateInDeviceIndependentUnits.X * PixelDensity,
            coordinateInDeviceIndependentUnits.Y * PixelDensity);
    }

    /// <summary>
    /// Converts coordinates in raw pixels to device independent units (or DIP or DP).
    /// </summary>
    /// <param name="coordinateInPixels">Coordinate in pixels</param>
    /// <returns>Coordinate in device independent units (or DIP or DP)</returns>
    public MPoint ToCoordinateInDeviceIndependentUnits(MPoint coordinateInPixels)
    {
        return new MPoint(coordinateInPixels.X / PixelDensity, coordinateInPixels.Y / PixelDensity);
    }
}
