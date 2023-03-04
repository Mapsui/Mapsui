using System.ComponentModel;

namespace Mapsui;

public interface IReadOnlyViewport : IViewportState
{
    event PropertyChangedEventHandler ViewportChanged;

     ViewportState State { get; }
}
