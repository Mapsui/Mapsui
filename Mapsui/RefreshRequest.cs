namespace Mapsui;

/// <summary>
/// Describes what needs to be redrawn on the next render cycle.
/// Either the full viewport (<see cref="Full"/>) or a specific world-coordinate dirty rectangle.
/// Accumulate consecutive requests with <see cref="Accumulate"/>.
/// </summary>
public sealed record RefreshRequest
{
    /// <summary>Singleton representing a full-viewport refresh.</summary>
    public static readonly RefreshRequest Full = new();

    /// <summary>
    /// The world-coordinate region to redraw, or <see langword="null"/> when this is a full refresh.
    /// </summary>
    public MRect? DirtyRect { get; }

    /// <summary>Returns <see langword="true"/> when the entire viewport must be redrawn.</summary>
    public bool IsFullRefresh => DirtyRect == null;

    /// <summary>Creates a full-viewport refresh request.</summary>
    private RefreshRequest() { }

    /// <summary>Creates a partial refresh request for the given world-coordinate rectangle.</summary>
    public RefreshRequest(MRect dirtyRect) => DirtyRect = dirtyRect;

    /// <summary>
    /// Returns a <see cref="RefreshRequest"/> that covers the union of this request and
    /// <paramref name="other"/>. If either is a full refresh the result is always
    /// <see cref="Full"/>; otherwise the dirty rectangles are joined.
    /// </summary>
    public RefreshRequest Accumulate(RefreshRequest other)
    {
        if (IsFullRefresh || other.IsFullRefresh) return Full;
        return new RefreshRequest(DirtyRect!.Join(other.DirtyRect!));
    }
}
