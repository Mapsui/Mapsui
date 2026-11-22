using System;
using System.Threading;

namespace Mapsui.Utilities;

/// <summary>
/// Hands an immutable state object from any thread to the render pipeline without locks.
/// </summary>
/// <remarks>
/// Public members of a layer can be called from any thread, for example from a GPS callback, while
/// the render pipeline reads the layer's styles and features. Instead of locking, callers describe
/// what they want in an immutable state record and publish it with <see cref="Update"/>. The render
/// pipeline takes the latest state in <c>UpdateAnimations</c> through <see cref="TryTake"/> and is the
/// only code that touches the mutable styles and features. Updates that arrive between two frames
/// collapse into one; the last one wins. <see cref="Latest"/> always returns what was requested last,
/// so a caller that sets a value and reads it back sees its own value immediately.
/// </remarks>
/// <typeparam name="T">An immutable type, typically a record.</typeparam>
public sealed class StateHandover<T> where T : class
{
    private T _latest;
    private T _applied;

    /// <param name="initial">The initial state. It counts as applied, so the render pipeline must already match it.</param>
    public StateHandover(T initial)
    {
        ArgumentNullException.ThrowIfNull(initial);
        _latest = initial;
        _applied = initial;
    }

    /// <summary>
    /// The most recently requested state. Safe to read from any thread.
    /// </summary>
    public T Latest => Volatile.Read(ref _latest);

    /// <summary>
    /// The state the render pipeline applied last. Read it on the render pipeline only; it is written by
    /// <see cref="TryTake"/> without synchronisation.
    /// </summary>
    public T Applied => _applied;

    /// <summary>
    /// Publishes a new state derived from the latest one. Safe to call from any thread; when several threads
    /// update at the same time every change is applied, so <paramref name="change"/> may run more than once
    /// and must not have side effects.
    /// </summary>
    /// <returns>The state that was replaced and the state that was published.</returns>
    public (T Previous, T Latest) Update(Func<T, T> change)
    {
        ArgumentNullException.ThrowIfNull(change);
        while (true)
        {
            var previous = Volatile.Read(ref _latest);
            var latest = change(previous);
            if (ReferenceEquals(latest, previous)) return (previous, previous);
            if (ReferenceEquals(Interlocked.CompareExchange(ref _latest, latest, previous), previous))
                return (previous, latest);
        }
    }

    /// <summary>
    /// Takes the latest state when it differs from the one applied last. Call this from the render pipeline
    /// only, typically at the start of <c>UpdateAnimations</c>.
    /// </summary>
    /// <param name="previous">The state applied before.</param>
    /// <param name="latest">The state to apply now.</param>
    /// <returns>True when there is a new state to apply.</returns>
    public bool TryTake(out T previous, out T latest)
    {
        latest = Volatile.Read(ref _latest);
        previous = _applied;
        if (ReferenceEquals(latest, previous)) return false;
        _applied = latest;
        return true;
    }
}
