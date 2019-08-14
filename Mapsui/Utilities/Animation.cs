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

        public Animation(long duration)
        {
            Duration = duration;

            // Create timer for animation
            _timer = new Timer
            {
                Interval = 16,
                AutoReset = true
            };
            _timer.Elapsed += HandleTimerElapse;
        }

        public EventHandler<AnimationEventArgs> Started { get; set; }
        public EventHandler<AnimationEventArgs> Stopped { get; set; }
        public EventHandler<AnimationEventArgs> Ticked { get; set; }

        /// <summary>
        /// Duration of the whole animation cycle in milliseconds
        /// </summary>
        public long Duration { get; } = 300;

        /// <summary>
        /// Animations, that should be made
        /// </summary>
        public List<AnimationEntry> Entries { get; } = new List<AnimationEntry>();

        /// <summary>
        /// True, if animation is running
        /// </summary>
        public bool IsRunning { get => _timer != null && _timer.Enabled; }

        public void Start()
        {
            if (IsRunning)
            {
                Stop(false);
            }

            // Animation in ticks;
            _durationTicks = Duration * Stopwatch.Frequency / 1000;

            _stopwatch = Stopwatch.StartNew();
            _stopwatchStart = _stopwatch.ElapsedTicks;
            _timer.Start();

            Started?.Invoke(this, new AnimationEventArgs(0));
        }

        /// <summary>
        /// Stop a running animation if there is one
        /// </summary>
        /// <param name="gotoEnd">Should final of each list entry be called</param>
        public void Stop(bool gotoEnd = true)
        {
            if (!_timer.Enabled)
                return;

            _timer.Stop();
            _stopwatch.Stop();

            double ticks = _stopwatch.ElapsedTicks - _stopwatchStart;
            var value = ticks / _durationTicks;

            if (gotoEnd)
            {
                foreach(var entry in Entries)
                {
                    entry.Final();
                }
            }

            Stopped?.Invoke(this, new AnimationEventArgs(value));
        }

        /// <summary>
        /// Timer tick for animation
        /// </summary>
        /// <param name="sender">Sender of this tick</param>
        /// <param name="e">Timer tick arguments</param>
        private void HandleTimerElapse(object sender, ElapsedEventArgs e)
        {
            double ticks = _stopwatch.ElapsedTicks - _stopwatchStart;
            var value = ticks / _durationTicks;

            if (value >= 1.0)
            {
                Stop(true);
                return;
            }

            // Calc new values
            foreach(var entry in Entries)
            {
                if (value >= entry.AnimationStart && value <= entry.AnimationEnd)
                    entry.Tick(value);
            }

            Ticked?.Invoke(this, new AnimationEventArgs(value));
        }
    }
}
