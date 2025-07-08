using Mapsui.Logging;
using System;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

/// <summary>
/// Makes sure a method is always called 'MillisecondsToDelay' after the previous call.
/// </summary>
public class Throttler
{
    private long? _ticksPreviousCall;
    private readonly LatestMailbox<Func<Task>> _mailbox = new();

    /// <summary>
    /// Schedules the specified asynchronous function to be executed after a minimum delay,
    /// ensuring that at least <paramref name="delayBetweenCalls"/> milliseconds have elapsed since the previous execution.
    /// If multiple calls are made in quick succession, only the most recent is executed.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="delayBetweenCalls">The minimum number of milliseconds between two executions.</param>
    public async Task ExecuteAsync(Func<Task> func, int delayBetweenCalls)
    {
        _mailbox.Overwrite(func);
        await CallAsync(delayBetweenCalls).ConfigureAwait(false);
    }

    /// <summary>
    /// Schedules the specified action to be executed after a minimum delay,
    /// ensuring that at least <paramref name="delayBetweenCalls"/> milliseconds have elapsed since the previous execution.
    /// If multiple calls are made in quick succession, only the most recent is executed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="delayBetweenCalls">The minimum number of milliseconds between two executions.</param>
    public async Task ExecuteAsync(Action action, int delayBetweenCalls)
    {
        _mailbox.Overwrite(async () => { action(); await Task.CompletedTask.ConfigureAwait(false); });
        await CallAsync(delayBetweenCalls);
    }

    private async Task CallAsync(int delayBetweenCalls)
    {
        try
        {
            while (_mailbox.TryTake(out var action))
            {
                // If there are multiple actions queued, we only execute the last one.
                // The previous ones will be discarded.
                // This ensures that only the latest action is executed after the delay.
                if (_ticksPreviousCall is not null) // Only wait if there was a previous call
                {
                    var millisecondsPassedSinceLastCall = Environment.TickCount64 - _ticksPreviousCall.Value;
                    var ticksToWait = (int)Math.Max(delayBetweenCalls - millisecondsPassedSinceLastCall, 0);
                    if (ticksToWait > 0)
                        await Task.Delay(ticksToWait).ConfigureAwait(false);
                }
                await action().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Error in delayed action: {ex.Message}");
        }
        finally
        {
            _ticksPreviousCall = Environment.TickCount64;
        }
    }
}
