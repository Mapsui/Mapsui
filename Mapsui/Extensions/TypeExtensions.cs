using System;

using Mapsui.Utilities;

namespace Mapsui.Extensions;

public static class TypeExtensions
{
    // This method can be useful. I don't like to use it in all ImageSource assignments because
    // it makes the code slightly harder to understand. We should perhaps have one samples to show
    // how this can make is a bit easier to avoid errors.
    public static Uri LoadImageSource(this Type typeInAssemblyOfEmbeddedResource, string relativePathToEmbeddedResource)
    {
        return EmbeddedResourceLoader.GetResourceUri(typeInAssemblyOfEmbeddedResource, relativePathToEmbeddedResource);
    }
}
