using System;

// ReSharper disable once CheckNamespace
namespace Mapsui.Samples.Common;

public interface IMapViewSample : IMapControlSample
{
    bool OnClick(object? sender, EventArgs args);
}
