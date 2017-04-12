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
        private static readonly Task CompletedTask = Task.FromResult(false);

        private readonly TimerCallback _callback;
        private Task _delay;
        private bool _disposed;
        private readonly int _period;
        private readonly object _state;
        private CancellationTokenSource _tokenSource;
        // I (pauldendulk) introduced this _syncRoot because _tokenSource could 
        // become null through a Cancel call on another thread. I have not given
        // it much thought or did thorough testing.
        private readonly object _syncRoot = new object();

        public Timer(TimerCallback callback, int period, object state = null)
        {
            _callback = callback;
            _period = period;
            _state = state;
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

        private void Dispose(bool cleanUpManagedObjects)
        {
            if (cleanUpManagedObjects)
                Cancel();
            _disposed = true;
        }

        public void Restart(int dueTime = 0)
        {
            Cancel();
            Start(dueTime);
        }

        public void Start(int dueTime = 0)
        {
            if (dueTime >= 0)
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
                    if (_tokenSource != null)
                    {
                        if (dueTime > 0)
                            _delay = Task.Delay(dueTime, _tokenSource.Token);
                        else
                            _delay = CompletedTask;
                        _delay.ContinueWith(t => tick(), _tokenSource.Token);
                    }
                }
            }
        }

        public void Cancel()
        {
            lock (_syncRoot)
            {
                if (_tokenSource == null) return;

                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }
        }
    }
}