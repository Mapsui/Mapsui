using System.ComponentModel;

namespace Mapsui;

public class ViewportChangedEventArgs : PropertyChangedEventArgs
{
    public ViewportChangedEventArgs(Viewport oldViewport) : base(nameof(Viewport))
    {
        OldViewport = oldViewport;
    }

    public Viewport OldViewport { get; }
}
