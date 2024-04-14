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
            var result1 = EmbeddedResourceLoader.Load(relativePathToEmbeddedResource, typeInAssemblyOfEmbeddedResource);
            var result = result1;
#pragma warning restore IDISP001 // Dispose created.            
            bitmapId = bitmapRegistry.Register(result, fullName);
            return bitmapId;
        }

        return bitmapId;
    }

    public static Uri LoadBitmapPath(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        return EmbeddedResourceLoader.GetResourceUri(typeInAssemblyOfEmbeddedResource, relativePathToEmbeddedResource);
    }
}
