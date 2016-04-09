using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mapsui.Utilities
{
    public static class EmbeddedResourceLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativePathToEmbeddedResource">This is the path of the resource without the assemlby path but including 
        /// possible project folders. So if an image 'myimage.png' is in a project folders 'images' the path is
        /// 'images.myimage.png'. Resources always uses '.' as separators. </param>
        /// <param name="typeInAssemblyOfEmbeddedResource">This should be a type that is in the same assembly
        /// as the EmbeddedResource. It is used to infer the full path and is necessary to load the resource.</param>
        /// <returns></returns>
        public static Stream Load(string relativePathToEmbeddedResource, Type typeInAssemblyOfEmbeddedResource)
        {
            var assembly = typeInAssemblyOfEmbeddedResource.GetTypeInfo().Assembly;
            var fullName = GetAssemblyName(assembly) + "." + relativePathToEmbeddedResource;
            var result = assembly.GetManifestResourceStream(fullName);
            if (result == null) throw new Exception(ConstructExceptionMessage(relativePathToEmbeddedResource, assembly));
            return result;
        }

        private static string ConstructExceptionMessage(string path, Assembly assembly)
        {
            const string format = "The resource name '{0}' was not found in assembly '{1}'.";
            var message = string.Format(format, path, GetAssemblyName(assembly));

            var resourceNames = assembly.GetManifestResourceNames();
            if (resourceNames.Length == 0)
            {
                message += " There are no resources in this assembly.";
                Debug.WriteLine(message);
                return message;
            }

            var similarNames = resourceNames.Where(name => path.ToLower().Split('.')
                .Any(name.ToLower().Contains)).ToArray();

            if (similarNames.Length <= 0) return message;

            var nameLength = GetAssemblyName(assembly).Length;
            similarNames = similarNames.Select(fullName => fullName.Remove(0, nameLength + 1)).ToArray();
            message += " Did you mean: " + string.Join("\n ", similarNames.ToArray()) + ".";
            Debug.WriteLine(message);
            return message;
        }

        private static string GetAssemblyName(Assembly assembly)
        {
            return new AssemblyName(assembly.FullName).Name;
        }
    }
}
