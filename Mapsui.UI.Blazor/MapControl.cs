using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using System.Diagnostics;
using SkiaSharp;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Blazor
{
    public sealed partial class MapControl : IMapControl, IDisposable
    {
        private SKImageInfo? _canvasSize;
        private IRenderer _renderer = new MapRenderer();
        private bool _onloaded;

        public async void OpenBrowser(string url)
        {
            await JsRuntime.InvokeAsync<object>("open", new object?[]{ url, "_blank" });
        }

        public void Dispose()
        {
            CommonDispose(true);
        }

        public float PixelDensity => GetPixelDensity();

        public float ViewportWidth =>  _canvasSize?.Width ?? 0; 
        public float ViewportHeight => _canvasSize?.Height ?? 0;
    }
}
