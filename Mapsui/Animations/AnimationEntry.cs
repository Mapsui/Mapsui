using System;

namespace Mapsui.Utilities;

public class AnimationEntry<T>
{
    private readonly double _animationDelta;
    private readonly Action<T, AnimationEntry<T>, double>? _tick;
    private readonly Action<T, AnimationEntry<T>>? _final;

    public AnimationEntry(object start, object end,
        double animationStart = 0, double animationEnd = 1,
        Easing? easing = null,
        bool repeat = false,
        Action<T, AnimationEntry<T>, double>? tick = null,
        Action<T, AnimationEntry<T>>? final = null)
    {
        AnimationStart = animationStart;
        AnimationEnd = animationEnd;

        Start = start;
        End = end;

        Easing = easing ?? Easing.Linear;
        Repeat = repeat;

        _animationDelta = AnimationEnd - AnimationStart;

        _tick = tick;
        _final = final;
    }

    /// <summary>
    /// When this animation starts in animation cycle. Value between 0 and 1.
    /// </summary>
    public double AnimationStart { get; }

    /// <summary>
    /// When this animation ends in animation cycle. Value between 0 and 1.
    /// </summary>
    public double AnimationEnd { get; }

    /// <summary>
    /// Object holding the starting value
    /// </summary>
    public object Start { get; }

    /// <summary>
    /// Object holding the end value
    /// </summary>
    public object End { get; }

    /// <summary>
    /// Easing to use for this animation
    /// </summary>
    public Easing Easing { get; }

    /// <summary>
    /// Is this a repeating animation that starts over and over again
    /// </summary>
    public bool Repeat { get; }

    /// <summary>
    /// Time, where this AnimationEntry has started
    /// </summary>
    internal long StartTicks { get; set; }

    /// <summary>
    /// Time, where this AnimationEntry should end
    /// </summary>
    internal long EndTicks { get; set; }

    /// <summary>
    /// Lengths of this AnimationEntry in ticks
    /// </summary>
    internal long DurationTicks { get; set; }

    /// <summary>
    /// Called when a value should changed
    /// </summary>
    /// <param name="value">Position in animation cycle between 0 and 1</param>
    internal bool Tick(T target, double value)
    {
        // Each tick gets a value between 0 and 1 for its own cycle
        // Its independent from the global animation cycle
        var v = (value - AnimationStart) / _animationDelta;

        if (_tick != null)
        {
            _tick(target, this, v);
            return true;
        }

        return false;
    }

    /// <summary>
    /// When Done is true the AnimationEntry can removed. The Animation class will set this to true.
    /// </summary>
    internal bool Done { get; set; }

    /// <summary>
    /// Called when the animation cycle is at the end
    /// </summary>
    internal void Final(T target)
    {
        if (_final != null)
        {
            _final(target, this);
        }
    }
}
