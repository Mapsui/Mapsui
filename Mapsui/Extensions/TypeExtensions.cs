using System;
using System.IO;
using System.Reflection;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Extensions;

public static class TypeExtensions
{
    public static int LoadBitmapId(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        var fullName = assembly.GetFullName(relativePathToEmbeddedResource);
        if (!BitmapRegistry.Instance.TryGetBitmapId(fullName, out var bitmapId))
        {
            var result = LoadBitmap(typeInAssemblyOfEmbeddedResource, relativePathToEmbeddedResource);
            bitmapId = BitmapRegistry.Instance.Register(result, fullName);
            return bitmapId;
        }

        return bitmapId;
    }

    public static Stream LoadBitmap(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        var result = EmbeddedResourceLoader.Load(relativePathToEmbeddedResource, typeInAssemblyOfEmbeddedResource);
        return result;
    }
}
