using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using System.Diagnostics;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Blazor
{
    public sealed partial class MapControl : IMapControl, IDisposable
    {

        private IRenderer _renderer = new MapRenderer();

        public void OpenBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                // The default for this has changed in .net core, you have to explicitly set if to true for it to work.
                UseShellExecute = true
            });
        }

        public void Dispose()
        {
            _map?.Dispose();
            _map = null;

            CommonDispose(true);
        }

        public void RunOnUIThread(Action action)
        {
            action();
        }


        public float PixelDensity => GetPixelDensity();

        public float ViewportWidth =>  Width;
        public float ViewportHeight => Height;
    }
}
