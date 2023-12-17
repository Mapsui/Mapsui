using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Tests.Utilities;

public static class AssemblyInfo
{
    public static string AssemblyDirectory
    {
        get
        {
            var path = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(path)!;
        }
    }
}
