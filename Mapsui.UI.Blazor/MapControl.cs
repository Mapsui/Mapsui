using System.Diagnostics;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.UI.Blazor
{
    public partial class MapControl : IMapControl, IDisposable
    {
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

            CommonDispose(true);
        }
    }
}
