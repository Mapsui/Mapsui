using System;
using System.Collections.Generic;

namespace Mapsui.UI.Utils
{
    public class FlingTracker
    {
        const int maxSize = 2;
        const long maxTicks = 200 * 10000;  // Use only events from the last 200 ms

        readonly Dictionary<long, Queue<(double x, double y, long time)>> events;

        public FlingTracker()
        {
            events = new Dictionary<long, Queue<(double x, double y, long time)>>();
        }

        public void AddEvent(long id, Geometries.Point location, long ticks)
        {
            // Save event data
            if (!events.ContainsKey(id))
            {
                events.Add(id, new Queue<(double x, double y, long time)>());
            }

            events[id].Enqueue((location.X, location.Y, ticks));
            
            // Check, if we at the end of array
            if (events[id].Count > maxSize)
            {
                events[id].Dequeue();
            }
        }

        // STOP TRACKING THIS ONE
        public void RemoveId(long id)
        {
            if (events.ContainsKey(id))
            {
                events.Remove(id);
            }
        }

        public void Clear()
        {
            events.Clear();
        }

        public (double vx, double vy) CalcVelocity(long id, long now)
        {
            double velocityX = 0;
            double velocityY = 0;

            if (!events.ContainsKey(id) || events[id].Count != 2)
                return (0d, 0d);

            var eventQueue = events[id];
            var eventsArray = eventQueue.ToArray();

            (var lastX, var lastY, var lastTime) = eventsArray[0];
            (var nowX, var nowY, var nowTime) = eventsArray[1];

            // Only calc velocities for last maxTicks ticks
            if (now - lastTime < maxTicks)
            {
                // Calc velocity in pixel per sec
                velocityX = (nowX - lastX) * 10000000 / (nowTime - lastTime);
                velocityY = (nowY - lastY) * 10000000 / (nowTime - lastTime);
            }
            
            return (velocityX, velocityY);
        }
    }
}
