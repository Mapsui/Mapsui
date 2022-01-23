﻿using System.Reflection;

namespace Mapsui.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetAssemblyName(this Assembly assembly)
        {
            return new AssemblyName(assembly.FullName).Name;
        }

        public static string GetFullName(this Assembly assembly, string relativePathToEmbeddedResource)
        {
            var fullName = GetAssemblyName(assembly) + "." + relativePathToEmbeddedResource;
            return fullName;
        }
    }
}
