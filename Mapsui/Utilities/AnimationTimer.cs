using Mapsui.UI;
using System.Timers;

namespace Mapsui.Utilities
{
    public class AnimationTimer : IAnimationTimer
    {
        private readonly Timer timer;
        private readonly IMapControl mapControl;
        private int counter = 0;

        public int Duration { get; }

        public bool IsRunning { get => timer.Enabled; }

        public AnimationTimer(IMapControl control, int duration = 16)
        {
            mapControl = control;
            Duration = duration;

            // Create timer for animation
            timer = new Timer
            {
                Interval = duration,
                AutoReset = true,
            };
            timer.Elapsed += HandleTimerElapse;
        }

        /// <summary>
        /// Start timer
        /// </summary>
        public void Start()
        {
            if (++counter == 1)
                timer.Start();
        }

        /// <summary>
        /// Stop timer
        /// </summary>
        public void Stop()
        {
            if (--counter == 0)
                timer.Stop();
        }

        /// <summary>
        /// Timer tick
        /// </summary>
        /// <param name="sender">Sender of this tick</param>
        /// <param name="e">Timer tick arguments</param>
        private void HandleTimerElapse(object sender, ElapsedEventArgs e)
        {
            mapControl?.RefreshGraphics();
        }
    }
}
