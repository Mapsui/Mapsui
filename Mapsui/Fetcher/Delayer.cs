using System;
using System.Threading;

namespace Mapsui.Fetcher
{
    /// <summary>
    /// Makes sure a method is always called 'MillisecondsToDelay' after the previous call.
    /// </summary>
    class Delayer
    {
        private readonly Timer _timer;
        private Action _action;
        private bool _waiting = false;

        /// <summary>
        /// The delay between two calls.
        /// </summary>
        public int MillisecondsToDelay { get; set; } = 500;

        public Delayer()
        {
            _timer = new Timer(FetchDelayTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Executes the method passed as argument with a possible delay. After a previous
        /// call the next call is delayed until 'MillisecondsToDelay' has passed.
        /// When ExecuteDelayed is called before the previous delayed action was executed 
        /// the previous one will be cancelled.
        /// </summary>
        /// <param name="action">The action to be executed after the possible delay</param>
        /// <remarks>When the previous call was more than 'MillisecondsToDelay' ago there will
        /// be no delay.</remarks>
        public void ExecuteDelayed(Action action)
        {                       
            if (_waiting)
            {
                // If waiting, just assign the action and wait for it to be called.
                _action = action;
            }
            else
            {
                // If not waiting just call the action.
                action();
                // Then wait for another time to check if more actions come in.
                StartWaiting();
            }
        }

        private void FetchDelayTimerElapsed(object state)
        {
            if (_action == null)
            {
                // The _action is null, so during the previous interval no new request came in,
                // so next time we don't have to wait.
                StopWaiting();
            }
            else
            {
                // Waiting is done, we can call the action.
                _action?.Invoke();
                // Set the action to null. This indicates there is no new request.
                _action = null;
                // Now we keep the timer running. It will stop if _action is still null.
            }
        }

        private void StartWaiting()
        {
            _waiting = true;
            _timer.Change(MillisecondsToDelay, MillisecondsToDelay);
        }

        private void StopWaiting()
        {
            _waiting = false;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}