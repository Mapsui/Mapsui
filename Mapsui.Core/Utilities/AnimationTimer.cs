using Mapsui.UI;
using System.Timers;

namespace Mapsui.Utilities
{
    public class AnimationTimer : IAnimationTimer
    {
        private readonly Timer _timer;
        private readonly IMapControl _mapControl;
        private int _counter;

        public int Duration { get; }

        public bool IsRunning => _timer.Enabled;

        public AnimationTimer(IMapControl control, int duration = 16)
        {
            _mapControl = control;
            Duration = duration;

            // Create timer for animation
            _timer = new Timer
            {
                Interval = duration,
                AutoReset = true,
            };
            _timer.Elapsed += HandleTimerElapse;
        }

        /// <summary>
        /// Start timer
        /// </summary>
        public void Start()
        {
            if (++_counter == 1)
                _timer.Start();
        }

        /// <summary>
        /// Stop timer
        /// </summary>
        public void Stop()
        {
            if (--_counter == 0)
                _timer.Stop();
        }

        /// <summary>
        /// Timer tick
        /// </summary>
        /// <param name="sender">Sender of this tick</param>
        /// <param name="e">Timer tick arguments</param>
        private void HandleTimerElapse(object sender, ElapsedEventArgs e)
        {
            _mapControl?.RefreshGraphics();
        }
    }
}
