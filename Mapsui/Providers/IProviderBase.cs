namespace Mapsui.Providers;

public interface IProviderBase
{
    /// <summary>
    /// The spatial reference ID (CRS)
    /// </summary>
    string? CRS { get; set; }

    /// <summary>
    /// <see cref="MRect"/> of data set
    /// </summary>
    /// <returns>BoundingBox</returns>
    MRect? GetExtent();
}