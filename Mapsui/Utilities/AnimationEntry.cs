using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Utilities
{
    public class AnimationEntry
    {
        private double _animationDelta;
        private Action<AnimationEntry, double> _tick;
        private Action<AnimationEntry> _final;

        public AnimationEntry(object start, object end, 
            double animationStart = 0, double animationEnd = 1, 
            Easing easing = null,
            Action<AnimationEntry, double> tick = null, 
            Action<AnimationEntry> final = null)
        {
            AnimationStart = animationStart;
            AnimationEnd = animationEnd;

            Start = start;
            End = end;

            Easing = easing ?? Easing.Linear;

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
        /// Called when a value should changed
        /// </summary>
        /// <param name="value">Position in animation cycle between 0 and 1</param>
        public void Tick(double value)
        {
            if (value < AnimationStart || value > AnimationEnd)
                return;

            // Each tick gets a value between 0 and 1 for its own cycle
            // Its independent from the global animation cycle
            var v = (value - AnimationStart) / _animationDelta;

            _tick(this, v);
        }

        /// <summary>
        /// Called when the animation cycle is at the end
        /// </summary>
        public void Final()
        {
            _final(this);
        }
    }
}
