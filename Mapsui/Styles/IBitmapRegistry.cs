using System;
using System.Threading.Tasks;

namespace Mapsui.Styles;

public interface IBitmapRegistry : IDisposable
{
    /// <summary>
    /// Register a new bitmap
    /// </summary>
    /// <param name="bitmapData">Bitmap data to register</param>
    /// <param name="key">key for accessing bitmap</param>
    /// <returns>Id of registered bitmap data</returns>
    int Register(object bitmapData, string? key = null);

    /// <summary>
    /// Register a new bitmap
    /// </summary>
    /// <param name="bitmapPath">Bitmap data to register</param>
    /// <returns>Id of registered bitmap data</returns>
    Task<int> RegisterAsync(Uri bitmapPath);

    /// <summary> Unregister an existing bitmap </summary>
    /// <param name="id">Id of registered bitmap data</param>
    /// <returns>The unregistered object</returns>
    object? Unregister(int id);

    /// <summary>
    /// Get bitmap data of registered bitmap
    /// </summary>
    /// <param name="id">Id of existing bitmap data</param>
    /// <returns></returns>
    object Get(int id);

    /// <summary>
    /// Set new bitmap data for a already registered bitmap
    /// </summary>
    /// <param name="id">Id of existing bitmap data</param>
    /// <param name="bitmapData">New bitmap data to replace</param>
    /// <returns>True, if replacing worked correct</returns>
    bool Set(int id, object bitmapData);

    /// <summary> Try Get Bitmap Id </summary>
    /// <param name="key">key</param>
    /// <param name="bitmapId">bitmap id</param>
    /// <returns>true if found</returns>
    bool TryGetBitmapId(string key, out int bitmapId);

    /// <summary>
    /// Check bitmap data for correctness
    /// </summary>
    /// <param name="bitmapData">Bitmap data to check</param>
    void CheckBitmapData(object bitmapData);
}
