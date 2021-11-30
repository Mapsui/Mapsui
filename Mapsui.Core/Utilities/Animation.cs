using System;
using System.Collections.Generic;

namespace Mapsui.Utilities
{
    public static class Animation
    {
        // Sync object for enries list
        private static readonly object _syncObject = new();

        /// <summary>
        /// List of all active animations
        /// </summary>
        private static readonly List<AnimationEntry> _entries = new();

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

            foreach (var entry in entries)
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
            lock (_syncObject)
            {
                if (_entries.Contains(entry))
                    Stop(entry, false);

                entry.StartTicks = ticks;
                entry.DurationTicks = duration * TimeSpan.TicksPerMillisecond;
                entry.EndTicks = entry.StartTicks + entry.DurationTicks;

                _entries.Add(entry);
            }
        }

        /// <summary>
        /// Stop a given AnimationEntry
        /// </summary>
        /// <param name="entry">AnimationEntry to stop</param>
        /// <param name="callFinal">Final function is called, if callFinal is true</param>
        public static void Stop(AnimationEntry entry, bool callFinal = true)
        {
            if (entry == null || !_entries.Contains(entry))
                return;

            if (callFinal)
                entry.Final();

            lock (_syncObject)
            {
                _entries.Remove(entry);
            }
        }

        /// <summary>
        /// Stop all AnimationEntrys in a given list
        /// </summary>
        /// <param name="entry">AnimationEntry to stop</param>
        /// <param name="callFinal">Final function is called, if callFinal is true</param>
        public static void Stop(IEnumerable<AnimationEntry> entries, bool callFinal = true)
        {
            foreach (var entry in entries)
                Stop(entry, callFinal);
        }

        /// <summary>
        /// Stop all animations
        /// </summary>
        /// <param name="callFinal">Final function is called, if callFinal is tru</param>
        public static void StopAll(bool callFinal = true)
        {
            Stop(_entries.ToArray(), callFinal);
        }

        /// <summary>
        /// Update all AnimationEntrys and check, if a redraw is needed
        /// </summary>
        /// <returns>True, if a redraw of the screen ist needed</returns>
        public static bool UpdateAnimations()
        {
            AnimationEntry[] entries;
            var ticks = DateTime.Now.Ticks;

            lock (_syncObject)
            {
                entries = _entries.ToArray();
            }

            if (entries.Length == 0)
                return false;

            var isRunning = false;

            for (int i = 0; i < entries.Length; i++)
            {
                if (ticks > entries[i].EndTicks)
                {
                    // Animation is at the end of duration
                    isRunning = true;
                    if (!entries[i].Repeat)
                    {
                        // Animation shouldn't be repeated, so remove it
                        Stop(entries[i], true);
                        continue;
                    }
                    // Set new values for repeating this animation
                    entries[i].StartTicks = entries[i].EndTicks;
                    entries[i].EndTicks = entries[i].StartTicks + entries[i].DurationTicks;
                }

                var value = (ticks - entries[i].StartTicks) / (double)entries[i].DurationTicks;

                if (value < entries[i].AnimationStart || value > entries[i].AnimationEnd)
                {
                    // Nothing to do before the animation starts or after animation ended
                    continue;
                }

                isRunning |= entries[i].Tick(value);
            }

            return isRunning;
        }
    }
}
