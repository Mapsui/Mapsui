using System.Threading.Tasks;

namespace Mapsui.Utilities;

public class AsyncCounterEvent
{
    private int _signalCount = 0;
    private TaskCompletionSource<bool>? _waiter = null;

    public Task WaitAsync()
    {
        lock (this)
        {
            if (_signalCount > 0)
            {
                _signalCount--;
                return Task.CompletedTask;
            }

            if (_waiter == null || _waiter.Task.IsCompleted)
                _waiter = new(TaskCreationOptions.RunContinuationsAsynchronously);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            return _waiter.Task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }
    }

    public void Set()
    {
        lock (this)
        {
            if (_waiter != null && !_waiter.Task.IsCompleted)
            {
                _waiter.SetResult(true);
                _waiter = null;
            }
            else
            {
                _signalCount++;
            }
        }
    }
}
