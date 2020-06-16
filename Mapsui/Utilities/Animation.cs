using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace Mapsui.Utilities
{
    public class Animation
    {
        private Timer _timer;
        private Stopwatch _stopwatch;
        private long _stopwatchStart;
        private long _durationTicks;
        private object _syncObject = new object();

        public Animation()
        {
            // Create timer for animation
            _timer = new Timer
            {
                Interval = 16,
                AutoReset = true
            };
            _timer.Elapsed += HandleTimerElapse;
            // Start the timer, it will keep running during the Animation's lifetime. 
            // If no animations are running the Ticked callback will not be called.
            _timer.Start();
        }

        public EventHandler<AnimationEventArgs> Started { get; set; }
        public EventHandler<AnimationEventArgs> Stopped { get; set; }
        public EventHandler<AnimationEventArgs> Ticked { get; set; }

        /// <summary>
        /// Duration of the whole animation cycle in milliseconds
        /// </summary>
        private long _duration = 300;

        /// <summary>
        /// Animations, that should be made
        /// </summary>
        private List<AnimationEntry> _entries { get; } = new List<AnimationEntry>();

        /// <summary>
        /// True, if animation is running
        /// </summary>
        public bool IsRunning { get; set; }

        private void Start()
        {
            if (IsRunning)
            {
                Stop(false);
            }

            // Animation in ticks;
            _durationTicks = _duration * Stopwatch.Frequency / 1000;

            _stopwatch = Stopwatch.StartNew();
            _stopwatchStart = _stopwatch.ElapsedTicks;
            IsRunning = true;
            Started?.Invoke(this, new AnimationEventArgs(0, ChangeType.Discrete));
        }

        /// <summary>
        /// Stop a running animation if there is one
        /// </summary>
        /// <param name="gotoEnd">Should final of each list entry be called</param>
        public void Stop(bool gotoEnd = true)
        {
            if (!IsRunning) return;

            _stopwatch.Stop();

            if (gotoEnd)
            {
                foreach (var entry in _entries)
                {
                    entry.Final();
                }
            }
            IsRunning = false;
        }

        /// <summary>
        /// Timer tick for animation
        /// </summary>
        /// <param name="sender">Sender of this tick</param>
        /// <param name="e">Timer tick arguments</param>
        private void HandleTimerElapse(object sender, ElapsedEventArgs e)
        {
            if (IsRunning)
            {
                var ticks = (_stopwatch.ElapsedTicks - _stopwatchStart) / _durationTicks;
                var changeType = (ticks >= 1.0) ? ChangeType.Discrete : ChangeType.Continuous;
                Ticked?.Invoke(sender, new AnimationEventArgs(ticks, changeType));
            }
        }

        public void UpdateAnimations()
        {
            if (!IsRunning) return;

            double ticks = _stopwatch.ElapsedTicks - _stopwatchStart;
            var value = ticks / _durationTicks;

            if (value >= 1.0)
            {
                Stop(true);
                return;
            }

            // Calc new values
            foreach (var entry in _entries)
            {
                if (value >= entry.AnimationStart && value <= entry.AnimationEnd)
                    entry.Tick(value);
            }
        }

        public void Start(List<AnimationEntry> entries, long duration)
        {
            lock (_syncObject)
            {
                _duration = duration;
                _entries.Clear();
                _entries.AddRange(entries);
                Start();
            }
        }
    }
}
