using System.ComponentModel;

namespace Mapsui;

public interface IReadOnlyViewport
{
    event PropertyChangedEventHandler ViewportChanged;

     ViewportState State { get; }
}
