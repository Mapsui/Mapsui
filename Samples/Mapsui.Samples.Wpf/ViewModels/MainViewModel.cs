using System;
using Mapsui.Tiling;

namespace Mapsui.Samples.Wpf.ViewModels;

public sealed class MainViewModel : IDisposable
{
    public Map Map { get; }

    public MainViewModel()
    {
        Map = new Map();

        Map.Layers.Add(OpenStreetMap.CreateTileLayer());
    }

    public void Dispose()
    {
        Map?.Dispose();
    }
}
