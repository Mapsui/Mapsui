using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using System.Diagnostics;
using SkiaSharp;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Blazor
{
    /// <summary>
    /// Helper Class to implement the MapControl features in Blazor
    /// https://stackoverflow.com/questions/60551182/how-to-add-a-partial-class-to-a-component-in-blazor-in-visual-studio-2019
    /// </summary>
    public abstract partial class MapControlBase : IMapControl, IDisposable
    {
        private protected IRenderer _renderer = new MapRenderer();

        public abstract float ViewportWidth { get; }
        public abstract float ViewportHeight { get; }

        public abstract void OpenBrowser(string url);
        public abstract float PixelDensity { get; }
        public abstract void Dispose();
        private protected abstract float GetPixelDensity();
        private protected abstract void RunOnUIThread(Action action);
    }
}
