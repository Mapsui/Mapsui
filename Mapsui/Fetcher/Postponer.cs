using System;
using System.Threading;

namespace Mapsui.Fetcher;

/// <summary>
/// Makes sure a method is always called 'MillisecondsToDelay' after the previous call.
/// </summary>
public class Postponer : IDisposable
{
    private readonly Timer _waitTimer;
    private Action? _action;
    private int _millisecondsToWait;

    public Postponer(int millisecondsToWait = 500)
    {
        _millisecondsToWait = millisecondsToWait;
        _waitTimer = new Timer(WaitTimerElapsed, null, millisecondsToWait, Timeout.Infinite);
    }

    public void ExecuteDelayed(Action action)
    {
        _action = action;
         Restart();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _waitTimer.Dispose();
        }
    }

    private void WaitTimerElapsed(object? state)
    {
        if (_action is not null)
        {
            _action?.Invoke();
            Restart();
        }
    }

    public void Restart()
    {
        _waitTimer.Change(_millisecondsToWait, Timeout.Infinite);
    }
}
