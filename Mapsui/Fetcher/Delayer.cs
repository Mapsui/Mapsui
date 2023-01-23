using System;
using System.Threading;

namespace Mapsui.Fetcher;

/// <summary>
/// Makes sure a method is always called 'MillisecondsToDelay' after the previous call.
/// </summary>
public class Delayer : IDisposable
{
    private readonly Timer _waitTimer;
    private Action? _action;
    private bool _waiting;

    /// <summary>
    /// The delay between two calls.
    /// </summary>
    public int MillisecondsToWait { get; set; } = 500;

    public bool StartWithDelay { get; set; } = false;

    public Delayer()
    {
        _waitTimer = new Timer(WaitTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Executes the method passed as argument with a possible delay. After a previous
    /// call the next call is delayed until 'MillisecondsToWait' has passed.
    /// When ExecuteRequest is called before the previous delayed action was executed 
    /// the previous one will be cancelled.
    /// </summary>
    /// <param name="action">The action to be executed after the possible delay</param>
    /// <remarks>When the previous call was more than 'MillisecondsToWait' ago there will
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
            // If not waiting call the action immediately.
            if (!StartWithDelay)
                action();
            else
                _action = action;
            // Then wait for another interval to check if more actions come in.
            StartWaiting();
        }
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
        if (_action != null)
        {
            // Waiting is done, we can call the action.
            _action?.Invoke();
            // Set the action to null. This indicates there is no new request.
            _action = null;
            // Now we keep the timer running. It will stop if _action is still null.
        }
        else
        {
            // The _action is null, so during the previous interval no new request came in.
            // Next time a new request comes in we don't have to wait.
            StopWaiting();
        }
    }

    private void StartWaiting()
    {
        _waiting = true;
        _waitTimer.Change(MillisecondsToWait, MillisecondsToWait);
    }

    private void StopWaiting()
    {
        _waiting = false;
        _waitTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }
}
