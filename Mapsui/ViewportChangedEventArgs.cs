using System;
using System.ComponentModel;

namespace Mapsui;

public class ViewportChangedEventArgs : PropertyChangedEventArgs
{
    public ViewportChangedEventArgs(Viewport previousViewport, Viewport viewport) : base(nameof(Viewport))
    {
        PreviousViewport = previousViewport;
        Viewport = viewport;
    }

    [Obsolete("Use PreviousViewport")]
    public Viewport OldViewport => PreviousViewport;

    public Viewport PreviousViewport { get; }

    public Viewport Viewport { get; init; } = new Viewport();
}
