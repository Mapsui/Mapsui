using System;

namespace Mapsui.Utilities;

public static class Caster
{
    public static T TryCastOrThrow<T>(object? obj) where T : class
    {
        if (obj is null)
            throw new InvalidCastException("Cannot cast because the value is null.");
        return obj as T ?? throw new InvalidCastException($"Expected type {nameof(T)} but was {obj.GetType()}.");
    }
}
