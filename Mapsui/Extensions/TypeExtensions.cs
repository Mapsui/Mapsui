using System;
using System.IO;
using System.Reflection;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Extensions;

public static class TypeExtensions
{
    public static int LoadBitmapId(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource, IBitmapRegistry bitmapRegistry)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        var fullName = assembly.GetFullName(relativePathToEmbeddedResource);
        if (bitmapRegistry.TryGetBitmapId(fullName, out var bitmapId))
        {
#pragma warning disable IDISP001 // Dispose created.
            var result = LoadBitmap(typeInAssemblyOfEmbeddedResource, relativePathToEmbeddedResource);
#pragma warning restore IDISP001 // Dispose created.            
            bitmapId = bitmapRegistry.Register(result, fullName);
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
