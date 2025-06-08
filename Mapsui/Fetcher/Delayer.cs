using Mapsui.Logging;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

/// <summary>
/// Makes sure a method is always called 'MillisecondsToDelay' after the previous call.
/// </summary>
public class Delayer
{
    private long? _ticksPreviousCall;

    // The Channel has a capacity of just one and if full will drop the oldest, so that
    // if the method on the queue is not in progress yet the new call will replace the waiting one.
    // This is to avoid requests of an outdated extent.
    private readonly Channel<Func<Task>> _queue = Channel.CreateBounded<Func<Task>>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });

    public Delayer() => _ = AddConsumerAsync(_queue);

    private static async Task AddConsumerAsync(Channel<Func<Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync().ConfigureAwait(false))
            await action().ConfigureAwait(false);
    }

    /// <summary>
    /// Schedules the specified asynchronous function to be executed after a minimum delay,
    /// ensuring that at least <paramref name="delayBetweenCalls"/> milliseconds have elapsed since the previous execution.
    /// If multiple calls are made in quick succession, only the most recent is executed.
    /// </summary>
    /// <param name="func">The asynchronous function to execute.</param>
    /// <param name="delayBetweenCalls">The minimum number of milliseconds between two executions.</param>
    /// <param name="minimumDelay">The minimum number of milliseconds to wait after calling this method before executing 
    /// <paramref name="func"/>. . This is useful when starting continuous changes, like dragging the map.</param>
    public void ExecuteDelayed(Func<Task> func, int delayBetweenCalls, int minimumDelay)
    {
        _ = _queue.Writer.TryWrite(() => CallAsync(func, delayBetweenCalls, minimumDelay));
    }

    /// <summary>
    /// Schedules the specified action to be executed after a minimum delay,
    /// ensuring that at least <paramref name="delayBetweenCalls"/> milliseconds have elapsed since the previous execution.
    /// If multiple calls are made in quick succession, only the most recent is executed.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="delayBetweenCalls">The minimum number of milliseconds between two executions.</param>
    /// <param name="minimumDelay">The minimum number of milliseconds to wait after calling this method before executing 
    /// <paramref name="action"/>. This is useful when starting continuous changes, like dragging the map.</param>
    public void ExecuteDelayed(Action action, int delayBetweenCalls, int minimumDelay)
    {
        _ = _queue.Writer.TryWrite(() => CallAsync(async () => { action(); await Task.CompletedTask.ConfigureAwait(false); }, delayBetweenCalls, minimumDelay));
    }

    private async Task CallAsync(Func<Task> action, int delayBetweenCalls, int minimumDelay)
    {
        try
        {
            if (minimumDelay > 0)
                await Task.Delay(minimumDelay).ConfigureAwait(false);
            if (_ticksPreviousCall is not null) // Only wait if there was a previous call
            {
                var millisecondsPassedSinceLastCall = Environment.TickCount64 - _ticksPreviousCall.Value;
                var ticksToWait = (int)Math.Max(delayBetweenCalls - millisecondsPassedSinceLastCall, 0);
                if (ticksToWait > 0)
                    await Task.Delay(ticksToWait).ConfigureAwait(false);
            }
            await action().ConfigureAwait(false);
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
