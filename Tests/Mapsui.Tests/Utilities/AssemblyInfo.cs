using System.IO;

namespace Mapsui.Tests.Utilities;

public static class AssemblyInfo
{
    public static string AssemblyDirectory
    {
        get
        {
            var path = System.AppContext.BaseDirectory;
            return Path.GetDirectoryName(path)!;
        }
    }
}
