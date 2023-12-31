// Found at https://gist.github.com/YARG/681f426b78af6d77baab

using System;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;

namespace Mapsui.UI;

internal delegate void TimerCallback(object state);

internal sealed class Timer : CancellationTokenSource, IDisposable
{
    public Timer(TimerCallback callback, object state, int dueTime, int period)
    {
        Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
        {
            var tuple = (Tuple<TimerCallback, object>?)s;

            while (true)
            {
                if (IsCancellationRequested)
                    break;
                Catch.TaskRun(() => tuple?.Item1(tuple.Item2));
                await Task.Delay(period);
            }
        }, Tuple.Create(callback, state), CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
            TaskScheduler.Default);
    }

    public new void Dispose() { Cancel(); }
}
