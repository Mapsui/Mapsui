using System.IO;

namespace Mapsui.Tiling.Utilities;

public static class HttpClientTools
{
    public static string GetDefaultApplicationUserAgent()
    {
        return $"user-agent-of-{Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName)}";
    }
}
