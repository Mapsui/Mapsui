using DotNext.Threading;
using Microsoft.JSInterop;

namespace Mapsui.UI.Blazor.Extensions;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public sealed class MapsuiJsInterop : IAsyncDisposable
{
    private readonly AsyncLazy<IJSObjectReference> _moduleTask;

    public MapsuiJsInterop(IJSRuntime jsRuntime)
    {
        if (jsRuntime == null)
        {
            throw new ArgumentNullException(nameof(jsRuntime));
        }

        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Mapsui.Blazor/mapsuiJsInterop.js").AsTask());
    }

    public async Task<BoundingClientRect> BoundingClientRectAsync(string elementId)
    {
        var module = await _moduleTask;
        return await module.InvokeAsync<BoundingClientRect>("getBoundingClientRect", elementId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask;
            await module.DisposeAsync();
        }
    }

    public async ValueTask DisableMouseWheelAsync(string elementId)
    {
        var module = await _moduleTask;
        await module.InvokeVoidAsync(@"disableMousewheelScroll", elementId);
    }

    public async ValueTask DisableTouchAsync(string elementId)
    {
        var module = await _moduleTask;
        await module.InvokeVoidAsync(@"disableTouch", elementId);
    }

    public async ValueTask<double> GetPixelDensityAsync()
    {
        var module = await _moduleTask;
        return await module.InvokeAsync<double>(@"getPixelDensity");
    }
}
