using System;

namespace Mapsui;

/// <summary>
/// Event args for <see cref="Map.RefreshGraphicsRequest"/>.
/// Carries a <see cref="RefreshRequest"/> describing what needs to be redrawn.
/// </summary>
public sealed class RefreshGraphicsEventArgs : EventArgs
{
    /// <summary>Describes the region to redraw. <see cref="RefreshRequest.Full"/> means redraw the entire viewport.</summary>
    public RefreshRequest Request { get; }

    public RefreshGraphicsEventArgs(RefreshRequest request) => Request = request;
}
