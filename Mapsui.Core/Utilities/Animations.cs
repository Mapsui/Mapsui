using System;
using System.Collections.Generic;

namespace Mapsui.Utilities
{
    public static class Animations
    {
        // Sync object for enries list
        private static readonly object _syncObject = new();

        /// <summary>
        /// List of all active animations
        /// </summary>
        private static readonly List<AnimationEntry> _entries = new();

        public static void Start(AnimationEntry entry, long duration)
        {
            Start(entry, duration, DateTime.Now.Ticks);
        }

        public static void Start(IEnumerable<AnimationEntry> entries, long duration)
        {
            // Start all animations in entries with the same ticks
            var ticks = DateTime.Now.Ticks;

            foreach (var entry in entries)
                Start(entry, duration, ticks);
        }

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

        public static void Stop(List<AnimationEntry> entries, bool callFinal = true)
        {
            foreach (var entry in entries)
                Stop(entry, callFinal);
        }

        public static bool UpdateAnimations()
        {
            AnimationEntry[] entries;

            lock (_syncObject)
            {
                entries = _entries.ToArray();
            }

            if (entries.Length == 0)
                return false;

            var isRunning = false;

            for (int i = 0; i < entries.Length; i++)
            {
                if (DateTime.Now.Ticks > entries[i].EndTicks)
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

                var value = (DateTime.Now.Ticks - entries[i].StartTicks) / (double)entries[i].DurationTicks;

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
