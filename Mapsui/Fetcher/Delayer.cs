using System;
using System.Threading;

namespace Mapsui.Fetcher
{
    class Delayer
    {
        private readonly Timer _timer;
        private Action _method;
        private bool _waiting = false;

        public int MillisecondsToDelay { get; set; } = 500;

        public Delayer()
        {
            _timer = new Timer(FetchDelayTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Executes the method passed as argument with a delay specified
        /// by the dueTime parameter. When the method is called before a
        /// previous delayed method was executed the previous one will be
        /// cancelled.
        /// </summary>
        /// <param name="method">The method to be executed after the delay</param>
        /// <param name="interval">The delay before the method is executed in milliseconds</param>
        /// <remarks>On the first call to ExecuteDelayed the dueTime parameter will be ignored and
        /// the delay will be zero.</remarks>
        public void ExecuteDelayed(Action method)
        {                       
            if (_waiting)
            {
                // If waiting, just assing the method and wait
                _method = method;
            }
            else
            {
                // If not waiting. Call the method 
                method();

                // Start waiting
                _waiting = true;
                _timer.Change(MillisecondsToDelay, MillisecondsToDelay);
            }
        }
        
        private void FetchDelayTimerElapsed(object state)
        {
            if (_method == null)
            {
                // The _method is null, so during the previous interval no new request came in,
                // so next time we don't have to wait.

                // Stop waiting
                _waiting = false;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                // Timer elaspsed.
                _method?.Invoke();
                _method = null;
                // Now we keep the timer running. It will stop if _method is still null.
            }
        }
    }
}