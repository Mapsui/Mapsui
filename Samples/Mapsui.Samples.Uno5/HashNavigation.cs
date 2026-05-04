namespace Mapsui.Samples.Uno5;

/// <summary>
/// Reads and writes the browser URL hash to allow bookmarking a specific sample on WebAssembly.
/// On all other platforms the methods are no-ops so no call sites need platform guards.
/// Hash format: #category=Animations&amp;sample=AnimatedPointsSample
/// </summary>
internal static class HashNavigation
{
    internal static (string? category, string? sample) Read()
    {
#if __WASM__
        var hash = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.hash");
        if (string.IsNullOrEmpty(hash))
            return (null, null);

        // Strip leading '#' then split key=value pairs manually
        var query = hash.TrimStart('#');
        string? category = null;
        string? sample = null;
        foreach (var pair in query.Split('&'))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2)
            {
                var key = System.Uri.UnescapeDataString(kv[0]);
                var value = System.Uri.UnescapeDataString(kv[1]);
                if (key == "category") category = value;
                else if (key == "sample") sample = value;
            }
        }
        return (category, sample);
#else
        return (null, null);
#endif
    }

    internal static void Write(string category, string sample)
    {
#if __WASM__
        var encoded = $"#category={System.Uri.EscapeDataString(category)}&sample={System.Uri.EscapeDataString(sample)}";
        Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.history.replaceState(null, '', '{encoded}')");
#endif
    }
}

