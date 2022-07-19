using System;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;

#pragma warning disable VSTHRD110 // Observe the awaitable result of this method call by awaiting it
#pragma warning disable 1587
/// <remark>
/// Found at https://gist.github.com/YARG/681f426b78af6d77baab
/// </remark>
#pragma warning restore 1587

namespace Mapsui.UI
{
    internal delegate void TimerCallback(object state);

    internal sealed class Timer : CancellationTokenSource, IDisposable
    {
        public Timer(TimerCallback callback, object state, int dueTime, int period)
        {
            Task.Delay(dueTime, Token).ContinueWith(async (t, s) => {
                var tuple = (Tuple<TimerCallback, object>?)s;

                while (true)
                {
                    if (IsCancellationRequested)
                        break;
#pragma warning disable CS4014 // Missing await #pragma directive
                    Catch.TaskRun(() => tuple?.Item1(tuple.Item2));
#pragma warning restore CS4014 // Missing await #pragma directive
                    await Task.Delay(period);
                }
            }, Tuple.Create(callback, state), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }

        public new void Dispose() { Cancel(); }
    }
}