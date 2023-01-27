using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mapsui.Extensions;
namespace Mapsui.Utilities;

public static class EmbeddedResourceLoader
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="relativePathToEmbeddedResource">This is the path of the resource without the assembly path but including 
    /// possible project folders. So if an image 'myimage.png' is in a project folders 'images' the path is
    /// 'images.myimage.png'. Resources always uses '.' as separators. </param>
    /// <param name="typeInAssemblyOfEmbeddedResource">This should be a type that is in the same assembly
    /// as the EmbeddedResource. It is used to infer the full path and is necessary to load the resource.</param>
    /// <returns></returns>
    public static Stream Load(string relativePathToEmbeddedResource, Type typeInAssemblyOfEmbeddedResource)
    {
        var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
        var fullName = assembly.GetFullName(relativePathToEmbeddedResource);
        var result = assembly.GetManifestResourceStream(fullName);
        if (result == null) throw new Exception(ConstructExceptionMessage(relativePathToEmbeddedResource, assembly));
        return result;
    }

    private static string ConstructExceptionMessage(string path, Assembly assembly)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"The resource name '{path}' was not found in assembly '{assembly.GetAssemblyName()}'.");

        // Get all resources from the assembly
        var resourceNames = assembly.GetManifestResourceNames();

        // Give feedback if there are no resources
        if (resourceNames.Length == 0)
        {
            stringBuilder.Append(" There are no resources in this assembly.");
            return stringBuilder.ToString();
        }

        // Give feedback if there are resources with similar names
        var similarNames = resourceNames.Where(name => path.ToLower().Split('.')
            .Any(s => name.ToLower().Contains(s.ToLower()))).ToArray();
        if (similarNames.Length <= 0) return stringBuilder.ToString();
        var nameLength = assembly.GetAssemblyName()?.Length ?? 0;
        similarNames = similarNames.Select(fullName => fullName.Remove(0, nameLength + 1)).ToArray();
        stringBuilder.Append(" Did you try to get any of these embedded resources: " + string.Join("\n ", similarNames.ToArray()) + ".");

        return stringBuilder.ToString();
    }
}
