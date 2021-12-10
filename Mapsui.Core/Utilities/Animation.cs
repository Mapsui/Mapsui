using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Utilities
{
    public static class Animation
    {
        /// <summary>
        /// Start a single AnimationEntry
        /// </summary>
        /// <param name="entry">AnimationEntry to start</param>
        /// <param name="duration">Duration im ms for the given AnimationEntry</param>
        public static void Start(AnimationEntry entry, long duration)
        {
            Start(entry, duration, DateTime.Now.Ticks);
        }

        /// <summary>
        /// Start a list of AnimationEntrys
        /// </summary>
        /// <remarks>All AnimationEntries are started at the same time.</remarks>
        /// <param name="entries">List of AnimationEntry to start</param>
        /// <param name="duration">Duration im ms for the given AnimationEntry</param>
        public static void Start(IEnumerable<AnimationEntry> entries, long duration)
        {
            // Start all animations in entries with the same ticks
            var ticks = DateTime.Now.Ticks;
            var copiedEntries = entries.ToList();

            foreach (var entry in copiedEntries)
                Start(entry, duration, ticks);
        }

        /// <summary>
        /// Start of a AnimationEntry with given duration at given ticks for StartTicks
        /// </summary>
        /// <remarks>When the ticks is given, more than one AnimationEntry with the same StartTicks could be started.</remarks>
        /// <param name="entry">AnimationEntry to start</param>
        /// <param name="duration">Duration im ms for the given AnimationEntry</param>
        /// <param name="ticks">StartTicks for this AnimationEntry</param>
        private static void Start(AnimationEntry entry, long duration, long ticks)
        {
            if (entry == null)
                return;

            entry.StartTicks = ticks;
            entry.DurationTicks = duration * TimeSpan.TicksPerMillisecond;
            entry.EndTicks = entry.StartTicks + entry.DurationTicks;
        }

        /// <summary>
        /// Stop all AnimationEntrys in a given list
        /// </summary>
        /// <param name="entry">AnimationEntry to stop</param>
        /// <param name="callFinal">Final function is called, if callFinal is true</param>
        public static void Stop(IEnumerable<AnimationEntry> entries, bool callFinal = true)
        {
            var copiedEntries = entries.ToList();

            foreach (var entry in copiedEntries)
                Stop(entry, callFinal);
        }

        /// <summary>
        /// Stop a given AnimationEntry
        /// </summary>
        /// <param name="entry">AnimationEntry to stop</param>
        /// <param name="callFinal">Final function is called, if callFinal is true</param>
        public static void Stop(AnimationEntry entry, bool callFinal = true)
        {
            if (entry == null)
                return;

            if (callFinal)
                entry.Final();
        }

        /// <summary>
        /// Update more than one AnimationEntry and check, if a redraw is needed
        /// </summary>
        /// <returns>True, if a redraw of the screen ist needed</returns>
        public static bool Update(IEnumerable<AnimationEntry> entries, long ticks, bool callFinal = true)
        {
            bool isRunning = false;
            var copiedEntries = entries.ToList();

            foreach (var entry in copiedEntries)
                isRunning |= Update(entry, ticks, callFinal);

            return isRunning;
        }

        /// <summary>
        /// Update AnimationEntry and check, if a redraw is needed
        /// </summary>
        /// <returns>True, if a redraw of the screen ist needed</returns>
        public static bool Update(AnimationEntry entry, long ticks, bool callFinal = true)
        {
            if (entry == null)
                return false;

            if (ticks > entry.EndTicks)
            {
                // Animation is at the end of duration
                if (!entry.Repeat && callFinal)
                {
                    // Animation shouldn't be repeated, so remove it
                    return entry.Final();
                }
                // Set new values for repeating this animation
                entry.StartTicks = entry.EndTicks;
                entry.EndTicks = entry.StartTicks + entry.DurationTicks;
            }

            var value = (ticks - entry.StartTicks) / (double)entry.DurationTicks;

            if (value < entry.AnimationStart || value > entry.AnimationEnd)
            {
                // Nothing to do before the animation starts or after animation ended
                return false;
            }

            return entry.Tick(value);
        }
    }
}
