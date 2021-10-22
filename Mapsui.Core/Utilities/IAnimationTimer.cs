namespace Mapsui.Utilities
{
    public interface IAnimationTimer
    {
        /// <summary>
        /// Time in milliseconds between ticks
        /// </summary>
        int Duration { get; }

        /// <summary>
        /// Is there running an animation
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Start the timer
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the timer
        /// </summary>
        void Stop();
    }
}
