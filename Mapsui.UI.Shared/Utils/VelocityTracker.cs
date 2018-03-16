using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.UI.Utils
{
    public class VelocityTracker
    {
        const int maxSize = 100;
        const long maxTicks = 200 * 10000;  // Use only events from the last 200 ms
        double[,] x;
        double[,] y;
        long[,] time;
        int next = 0;
        bool turn = false;

        public VelocityTracker()
        {
            x = new double[10,maxSize];
            y = new double[10, maxSize];
            time = new long[10, maxSize];
        }

        public void AddEvent(long id, Geometries.Point location, long ticks)
        {
            // Sorry, track only 10 positions/fingers
            if (id < 0 || id > 9)
                return;

            // Save event data
            x[id, next] = location.X;
            y[id, next] = location.Y;
            time[id, next] = ticks;

            // Check for next position
            next++;

            // Check, if we at the end of array
            if (next >= maxSize)
            {
                next = 0;
                turn = true;
            }
        }

        public void Clear()
        {
            next = 0;
            turn = false;
        }

        public (double vx, double vy) CalcVelocity(long id, long now)
        {
            double velocityX = 0;
            double velocityY = 0;
            int start = 0;
            int pos = 0;

            if (id >= 0 && id < 10)
            {
                long[] ticks = new long[maxSize - 1];
                double[] vx = new double[maxSize - 1];
                double[] vy = new double[maxSize - 1];

                if (turn)
                    start = next;

                double lastX = x[id, start];
                double lastY = y[id, start];
                long lastTime = time[id, start];

                start++;

                if (turn)
                {
                    for (int i = start; i < maxSize; i++)
                    {
                        // Only calc velocities for last maxTicks ticks
                        if (now - time[id, i] < maxTicks)
                        {
                            // Calc velocity in pixel per sec
                            ticks[pos] = pos > 0 ? ticks[pos - 1] + time[id, i] - lastTime : 0;
                            vx[pos] = (x[id, i] - lastX) * 10000000 / (time[id, i] - lastTime);
                            vy[pos] = (y[id, i] - lastY) * 10000000 / (time[id, i] - lastTime);

                            pos++;
                        }

                        lastX = x[id, i];
                        lastY = y[id, i];
                        lastTime = time[id, i];
                    }

                    start = 0;
                }

                for (int i = start; i < next; i++)
                {
                    // Only calc velocities for last maxTicks ticks
                    if (now - time[id, i] < maxTicks)
                    {
                        // Calc velocity in pixel per sec
                        ticks[pos] = pos > 0 ? ticks[pos - 1] + time[id, i] - lastTime : 0;
                        vx[pos] = (x[id, i] - lastX) * 10000000 / (time[id, i] - lastTime);
                        vy[pos] = (y[id, i] - lastY) * 10000000 / (time[id, i] - lastTime);

                        pos++;
                    }

                    lastX = x[id, i];
                    lastY = y[id, i];
                    lastTime = time[id, i];
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
