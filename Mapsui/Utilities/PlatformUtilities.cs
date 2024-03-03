using System;
using System.Threading.Tasks;

namespace Mapsui.Utilities;

public static class PlatformUtilities
{
    private static Func<string, Task> OpenInBrowserFunc { get; set; } = (url) => throw new Exception(
        $"The '{nameof(OpenInBrowserFunc)}' method needs to be assigned in the MapControl constructor with the platform" +
        $" specific implementation before calling it");

    public static void SetOpenInBrowserFunc(Func<string, Task> openInBrowserFunc)
    {
        ArgumentNullException.ThrowIfNull(openInBrowserFunc, nameof(openInBrowserFunc));
        OpenInBrowserFunc = openInBrowserFunc;
    }

    public static async Task OpenInBrowserAsync(string url)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        await OpenInBrowserFunc(url);
    }
}
