using Mapsui.Samples.Common;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui;

public interface IMapViewSample : IMapControlSample
{
    bool OnTap(object? s, MapClickedEventArgs e);

    bool UpdateLocation { get; }
}
