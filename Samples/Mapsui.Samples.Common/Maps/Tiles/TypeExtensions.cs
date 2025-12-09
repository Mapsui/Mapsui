using Mapsui.Extensions;
using System;
using System.Reflection;

namespace MarinerNotices.MapsuiBuilder.Extensions;

public static class TypeExtensions
{
    public static string GetImageSourcePath(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        return $"embedded://{assembly.GetFullName(relativePathToEmbeddedResource)}";
    }
}
