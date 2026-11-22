using System;
using System.Collections.Generic;

namespace Mapsui.Animations;

/// <summary>
/// Owns a set of running animations and removes each one as soon as it is finished.
/// </summary>
/// <remarks>
/// <see cref="Animation.UpdateAnimations{T}"/> advances the entries it is given but never removes a finished
/// entry, and a finished entry that stays in a list keeps running its final step on every frame. This list
/// removes finished entries itself, so a caller only adds and updates. It is not thread-safe: use it from the
/// render pipeline only, see the threading section in docs/general/markdown/mapsui-architecture-principles.md.
/// </remarks>
public sealed class AnimationList<T>
{
    private readonly List<AnimationEntry<T>> _entries = [];

    public int Count => _entries.Count;

    /// <summary>
    /// Starts the entry with the given duration and adds it to the list.
    /// </summary>
    public void Start(AnimationEntry<T> entry, long duration)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Animation.Start(entry, duration);
        _entries.Add(entry);
    }

    /// <summary>
    /// Stops the entry and removes it. Does nothing when the entry is null or not in the list.
    /// </summary>
    /// <param name="target">The object the animation works on.</param>
    /// <param name="entry">The entry to stop.</param>
    /// <param name="callFinal">True to jump to the end state by calling the entry's final step.</param>
    public void Stop(T target, AnimationEntry<T>? entry, bool callFinal = true)
    {
        if (entry is null || !_entries.Remove(entry)) return;
        Animation.Stop(target, entry, callFinal);
    }

    /// <summary>
    /// Advances all animations one step and removes the ones that finished.
    /// </summary>
    /// <returns>True while at least one animation is still running.</returns>
    public bool Update(T target)
    {
        if (_entries.Count == 0) return false;
        var result = Animation.UpdateAnimations(target, _entries);
        _entries.RemoveAll(e => e.Done);
        return result.IsRunning;
    }
}
