namespace Mapsui.Providers;

public interface IProjectingProvider : IProvider
{
    /// <summary>
    /// Queries whether a provider supports projection to a certain CRS.
    /// </summary>
    /// <param name="crs">The crs to project to</param>
    /// <returns>True if is does, false if it does not, null if it is unknown</returns>
    bool? IsCrsSupported(string crs);
}
