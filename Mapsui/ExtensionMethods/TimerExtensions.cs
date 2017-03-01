using Mapsui.Utilities;

// ReSharper disable CheckNamespace
namespace System.Threading.Timers
{
    public static class TimerExtensions
    {
        public static void Stop(this Timer timer)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}