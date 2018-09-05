using System;

namespace Mapsui.UI.Utils
{
    public class VelocityTracker
    {
        private const int MaxSize = 100;
        private const long MaxTicks = 200 * 10000;  // Use only events from the last 200 ms
        private readonly double[,] _x;
        private readonly double[,] _y;
        private readonly long[,] _time;
        private int _next;
        private bool _turn;

        public VelocityTracker()
        {
            _x = new double[10,MaxSize];
            _y = new double[10, MaxSize];
            _time = new long[10, MaxSize];
        }

        public void AddEvent(long id, Geometries.Point location, long ticks)
        {
            // Sorry, track only 10 positions/fingers
            if (id < 0 || id > 9)
                return;

            // Save event data
            _x[id, _next] = location.X;
            _y[id, _next] = location.Y;
            _time[id, _next] = ticks;

            // Check for next position
            _next++;

            // Check, if we at the end of array
            if (_next >= MaxSize)
            {
                _next = 0;
                _turn = true;
            }
        }

        public void Clear()
        {
            _next = 0;
            _turn = false;
        }

        public (double vx, double vy) CalcVelocity(long id, long now)
        {
            double velocityX = 0;
            double velocityY = 0;
            int start = 0;
            int pos = 0;

            if (id >= 0 && id < 10)
            {
                long[] ticks = new long[MaxSize - 1];
                double[] vx = new double[MaxSize - 1];
                double[] vy = new double[MaxSize - 1];

                if (_turn)
                    start = _next;

                double lastX = _x[id, start];
                double lastY = _y[id, start];
                long lastTime = _time[id, start];

                start++;

                if (_turn)
                {
                    for (int i = start; i < MaxSize; i++)
                    {
                        // Only calc velocities for last maxTicks ticks
                        if (now - _time[id, i] < MaxTicks)
                        {
                            // Calc velocity in pixel per sec
                            ticks[pos] = pos > 0 ? ticks[pos - 1] + _time[id, i] - lastTime : 0;
                            vx[pos] = (_x[id, i] - lastX) * 10000000 / (_time[id, i] - lastTime);
                            vy[pos] = (_y[id, i] - lastY) * 10000000 / (_time[id, i] - lastTime);

                            pos++;
                        }

                        lastX = _x[id, i];
                        lastY = _y[id, i];
                        lastTime = _time[id, i];
                    }

                    start = 0;
                }

                for (int i = start; i < _next; i++)
                {
                    // Only calc velocities for last maxTicks ticks
                    if (now - _time[id, i] < MaxTicks)
                    {
                        // Calc velocity in pixel per sec
                        ticks[pos] = pos > 0 ? ticks[pos - 1] + _time[id, i] - lastTime : 0;
                        vx[pos] = (_x[id, i] - lastX) * 10000000 / (_time[id, i] - lastTime);
                        vy[pos] = (_y[id, i] - lastY) * 10000000 / (_time[id, i] - lastTime);

                        pos++;
                    }

                    lastX = _x[id, i];
                    lastY = _y[id, i];
                    lastTime = _time[id, i];
                }

                double rsquard;
                double yintercept;

                (rsquard, yintercept, velocityX) = LinearRegression(ticks, vx, 0, pos);
                (rsquard, yintercept, velocityY) = LinearRegression(ticks, vy, 0, pos);

                // Convert ticks to seconds
                velocityX = velocityX.IsNanOrZero() ? 0 : velocityX * 10000000;
                velocityY = velocityY.IsNanOrZero() ? 0 : velocityY * 10000000;
            }

            return (velocityX, velocityY);
        }

        //
        // Found at https://stackoverflow.com/questions/15623129/simple-linear-regression-for-data-set
        //

        private (double rsquard, double yintercept, double slope) LinearRegression(long[] xVals, double[] yVals,
                                        int inclusiveStart, int exclusiveEnd)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);

            double rsquared = dblR * dblR;
            double yintercept = meanY - ((sCo / ssX) * meanX);
            double slope = sCo / ssX;

            return (rsquared, yintercept, slope);
        }
    }
}
