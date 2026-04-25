namespace Mapsui;

/// <summary>
/// Describes what needs to be redrawn on the next render cycle.
/// Either the full viewport (<see cref="Full"/>) or a partial dirty rectangle in a specific
/// <see cref="CoordinateSpace"/>. Accumulate consecutive requests with <see cref="Accumulate"/>.
/// </summary>
public sealed record RefreshRequest
{
    /// <summary>Singleton representing a full-viewport refresh.</summary>
    public static readonly RefreshRequest Full = new();

    /// <summary>
    /// The dirty region to redraw, or <see langword="null"/> when this is a full refresh.
    /// Its coordinate space is given by <see cref="CoordinateSpace"/>.
    /// </summary>
    public MRect? DirtyRect { get; }

    /// <summary>
    /// Whether <see cref="DirtyRect"/> is in world or screen coordinates.
    /// Irrelevant when <see cref="IsFullRefresh"/> is <see langword="true"/>.
    /// </summary>
    public CoordinateSpace CoordinateSpace { get; }

    /// <summary>Returns <see langword="true"/> when the entire viewport must be redrawn.</summary>
    public bool IsFullRefresh => DirtyRect == null;

    /// <summary>Creates a full-viewport refresh request.</summary>
    private RefreshRequest() { }

    /// <summary>Creates a partial refresh request for the given world-coordinate rectangle.</summary>
    public RefreshRequest(MRect dirtyRect) : this(dirtyRect, CoordinateSpace.World) { }

    /// <summary>Creates a partial refresh request for the given rectangle in the specified coordinate space.</summary>
    public RefreshRequest(MRect dirtyRect, CoordinateSpace coordinateSpace)
    {
        DirtyRect = dirtyRect;
        CoordinateSpace = coordinateSpace;
    }

    /// <summary>
    /// Returns a <see cref="RefreshRequest"/> that covers the union of this request and
    /// <paramref name="other"/>. If either is a full refresh, or the two requests use different
    /// coordinate spaces, the result is always <see cref="Full"/>; otherwise the dirty rectangles
    /// are joined within the shared coordinate space.
    /// </summary>
    public RefreshRequest Accumulate(RefreshRequest other)
    {
        if (IsFullRefresh || other.IsFullRefresh) return Full;
        // Rects from different coordinate spaces can't be unioned without a viewport.
        if (CoordinateSpace != other.CoordinateSpace) return Full;
        return new RefreshRequest(DirtyRect!.Join(other.DirtyRect!), CoordinateSpace);
    }
}
