using Mapsui.Samples.Common;
using System;

namespace Mapsui.Samples.Maui;

public interface IMapViewSample : IMapControlSample
{
    bool OnTap(object? sender, EventArgs args);

    bool UpdateLocation { get; }
}
