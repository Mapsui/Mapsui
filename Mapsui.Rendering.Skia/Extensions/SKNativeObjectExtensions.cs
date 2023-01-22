using System.Reflection;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class SKNativeObjectExtensions
{
    private static PropertyInfo? _disposedProperty;

    public static bool IsDisposed(this SKNativeObject? skNativeObject)
    {
        if (skNativeObject == null)
        {
            return false;
        }

        _disposedProperty ??= typeof(SKNativeObject).GetProperty("IsDisposed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (_disposedProperty?.GetValue(skNativeObject) is bool disposed)
        {
            return disposed;
        }

        return false;
    }
}
