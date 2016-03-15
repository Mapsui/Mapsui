using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Utilities
{
    public delegate void TimerCallback(object state);

    /// <summary>
    /// Taken from: http://stackoverflow.com/questions/22619896/problems-with-system-threading-timer-in-pcl-profile-78-warnings
    /// Thank you Daniel Henry
    /// </summary>
    public sealed class Timer : IDisposable
    {
        static readonly Task CompletedTask = Task.FromResult(false);
        readonly TimerCallback _callback;
        Task _delay;
        bool _disposed;
        int _period;
        readonly object _state;
        CancellationTokenSource _tokenSource;
        // I (pauldendulk) introduced this _syncRoot because _tokenSource could 
        // become null through a Cancel call on another thread. I have not given
        // it much thought or did thorough testing.
        readonly object _syncRoot = new object();

        public Timer(TimerCallback callback, object state, int dueTime, int period)
        {
            _callback = callback;
            _state = state;
            _period = period;
            Reset(dueTime);
        }

        public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
            : this(callback, state, (int)dueTime.TotalMilliseconds, (int)period.TotalMilliseconds)
        {
        }

        ~Timer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool cleanUpManagedObjects)
        {
            if (cleanUpManagedObjects)
                Cancel();
            _disposed = true;
        }

        public void Change(int dueTime, int period)
        {
            _period = period;
            Reset(dueTime);
        }

        public void Change(TimeSpan dueTime, TimeSpan period)
        {
            Change((int)dueTime.TotalMilliseconds, (int)period.TotalMilliseconds);
        }

        void Reset(int due)
        {
            Cancel();
            if (due >= 0)
            {
                lock (_syncRoot)
                {
                    _tokenSource = new CancellationTokenSource();
                }
                Action tick = null;
                tick = () =>
                {
                    Task.Run(() => _callback(_state));
                    lock (_syncRoot)
                    {
                        if (!_disposed && _period >= 0 && _tokenSource != null)
                        {
                            if (_period > 0)
                                _delay = Task.Delay(_period, _tokenSource.Token);
                            else
                                _delay = CompletedTask;
                            _delay.ContinueWith(t => tick(), _tokenSource.Token);
                        }
                    }
                };
                lock (_syncRoot)
                {
                    if (due > 0)
                        _delay = Task.Delay(due, _tokenSource.Token);
                    else
                        _delay = CompletedTask;
                    _delay.ContinueWith(t => tick(), _tokenSource.Token);
                }
            }
        }

        void Cancel()
        {
            lock (_syncRoot)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                    _tokenSource.Dispose();
                    _tokenSource = null;
                }
            }
        }
    }
}