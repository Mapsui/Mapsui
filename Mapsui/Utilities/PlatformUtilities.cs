using System;

namespace Mapsui.Utilities;

public static class PlatformUtilities
{
    private static Action<string> OpenInBrowserMethod { get; set; } = (url) => throw new Exception(
        $"The '{nameof(OpenInBrowserMethod)}' method needs to be assigned in the MapControl constructor with the platform" +
        $" specific implementation before calling it");

    public static void SetOpenInBrowserFunc(Action<string> openInBrowserMethod)
    {
        ArgumentNullException.ThrowIfNull(openInBrowserMethod, nameof(openInBrowserMethod));
        OpenInBrowserMethod = openInBrowserMethod;
    }

    public static void OpenInBrowser(string url)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));
        OpenInBrowserMethod(url);
    }
}
