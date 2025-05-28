using Mapsui.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Utilities;

public class AsyncAutoResetEvent(bool initialState = false)
{
    private static readonly ValueTask<bool> _completed = new(true);
    private readonly Queue<TaskCompletionSource<bool>> _waits = new();
    private bool _letThrough = initialState;

    public ValueTask<bool> WaitAsync()
    {
        lock (_waits)
        {
            if (_letThrough) // Let this one through
            {
                _letThrough = false; // But let the next one wait.
                return _completed;
            }
            else // Wait 
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waits.Enqueue(tcs);
                return new ValueTask<bool>(tcs.Task);
            }
        }
    }

    public void Set()
    {
        TaskCompletionSource<bool>? toRelease = null;
        lock (_waits)
        {
            if (_waits.Count > 0) // If one was waiting, let it through but the next one will have to wait.
            {
                if (_waits.Count > 1)
                    Logger.Log(LogLevel.Error, "More than one thread is waiting for the AsyncAutoResetEvent. It was not intended for this use.");
                toRelease = _waits.Dequeue(); // Let this one through.
                _letThrough = false; // Let the next one wait.
            }
            else if (!_letThrough) // If no one was waiting let the next one through.
                _letThrough = true;
        }
        toRelease?.SetResult(true);
    }
}
