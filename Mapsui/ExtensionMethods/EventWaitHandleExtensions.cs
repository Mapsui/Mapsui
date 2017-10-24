using System.Threading;

namespace Mapsui
{
    static class EventWaitHandleExtensions
    {
        public static void Stop(this EventWaitHandle waitHandle)
        {
            waitHandle.Reset();
        }

        public static void Go(this EventWaitHandle waitHandle)
        {
            waitHandle.Set();
        }
    }
}
