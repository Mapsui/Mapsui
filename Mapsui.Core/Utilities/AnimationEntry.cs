using System;

namespace Mapsui.Utilities
{
    public class AnimationEntry
    {
        private readonly double _animationDelta;
        private readonly Func<AnimationEntry, double, bool>? _tick;
        private readonly Func<AnimationEntry, bool>? _final;

        public AnimationEntry(object start, object end,
            double animationStart = 0, double animationEnd = 1,
            Easing? easing = null,
            bool repeat = false,
            Func<AnimationEntry, double, bool>? tick = null,
            Func<AnimationEntry, bool>? final = null,
            string name = "")
        {
            Name = name;

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
        /// Name of this AnimationEntry
        /// </summary>
        public string Name { get; }

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
        internal bool Tick(double value)
        {
            // Each tick gets a value between 0 and 1 for its own cycle
            // Its independent from the global animation cycle
            var v = (value - AnimationStart) / _animationDelta;

            if (_tick != null)
            {
                return _tick(this, v);
            }

            return false;
        }

        /// <summary>
        /// Called when the animation cycle is at the end
        /// </summary>
        internal bool Final()
        {
            if (_final != null)
            {
                return _final(this);
            }

            return false;
        }
    }
}
