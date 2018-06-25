using System;
using System.Threading;

namespace Mapsui.Fetcher
{
    class Delayer
    {
        private readonly Timer _timer;
        private Action _method;
        private bool _first = true;

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
        /// <param name="dueTime">The delay before the method is executed in milliseconds</param>
        /// <remarks>On the first call to ExecuteDelayed the dueTime parameter will be ignored and
        /// the delay will be zero.</remarks>
        public void ExecuteDelayed(Action method, int dueTime)
        {
            _method = method;
            
            if (_first) // On initial load we want a fast response
            {
                dueTime = 0;
                _first = false;
            }

            _timer.Change(dueTime, Timeout.Infinite);
        }
        
        private void FetchDelayTimerElapsed(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _method?.Invoke();
        }
    }
}