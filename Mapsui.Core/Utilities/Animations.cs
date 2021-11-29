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
            lock (_syncObject)
            {
                if (_entries.Contains(entry))
                    Stop(entry, false);

                entry.StartTicks = DateTime.Now.Ticks;
                entry.DurationTicks = duration * TimeSpan.TicksPerMillisecond;

                _entries.Add(entry);
            }
        }

        public static void Start(List<AnimationEntry> entries, long duration)
        {
            foreach (var entry in entries)
                Start(entry, duration);
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
                var value = (DateTime.Now.Ticks - entries[i].StartTicks) / (double)entries[i].DurationTicks;

                if (value < entries[i].AnimationStart)
                {
                    // Nothing to do before the animation starts
                    continue;
                }

                if (value > entries[i].AnimationEnd)
                {
                    // Animation is at its end, so remove it
                    isRunning = true;
                    Stop(entries[i], true);
                    continue;
                }

                isRunning |= entries[i].Tick(value);
            }

            return isRunning;
        }
    }
}
