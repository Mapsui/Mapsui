using System;

namespace Mapsui.Samples.Common;

public interface IMapViewSample : IMapControlSample
{
    bool OnClick(object? sender, EventArgs args);

    bool UpdateLocation { get; }
}
