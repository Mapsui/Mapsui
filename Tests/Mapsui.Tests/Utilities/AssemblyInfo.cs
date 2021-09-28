using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Desktop.Tests.Utilities
{
    public static class AssemblyInfo
    {
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}