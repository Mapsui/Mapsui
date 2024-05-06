using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

/// <summary>
/// Makes sure a method is always called 'MillisecondsToDelay' after the previous call.
/// </summary>
public class Delayer
{
    private int _ticksPreviousCall = 0;

    // The Channel has a capacity of just one and if full will drop the oldest, so that
    // if the method on the queue is not in progress yet the new call will replace the waiting one.
    // This is to avoid requests of an outdated extent.
    private readonly Channel<Func<Task>> _queue = Channel.CreateBounded<Func<Task>>(
        new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });

    public Delayer() => _ = AddConsumerAsync(_queue);

    /// <summary>
    /// The minimum delay between two calls.
    /// </summary>
    public int MillisecondsBetweenCalls { get; set; } = 500;
    /// <summary>
    /// The delay between the call to ExecuteDelayed and the actual call to the method.
    /// </summary>
    public int MillisecondsBeforeCall { get; set; } = 0;


    private static async Task AddConsumerAsync(Channel<Func<Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync().ConfigureAwait(false))
            await action().ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the method passed as argument with a possible delay. After a previous
    /// call the next call is delayed until 'MillisecondsToWait' has passed.
    /// When ExecuteRequest is called before the previous delayed action was executed 
    /// the previous one will be cancelled.
    /// </summary>
    /// <param name="func">The action to be executed after the possible delay</param>
    /// <remarks>When the previous call was more than 'MillisecondsToWait' ago there will
    /// be no delay.</remarks>
    public void ExecuteDelayed(Func<Task> func)
    {
        _queue.Writer.TryWrite(() => CallAsync(func));
    }

    public void ExecuteDelayed(Action action)
    {
        _queue.Writer.TryWrite(() => CallAsync(async () => { action(); await Task.CompletedTask.ConfigureAwait(false); }));
    }

    private async Task CallAsync(Func<Task> action)
    {
        if (MillisecondsBeforeCall > 0)
            await Task.Delay(MillisecondsBeforeCall).ConfigureAwait(false);
        var ticksToWait = Math.Max(MillisecondsBetweenCalls - (Environment.TickCount - _ticksPreviousCall), 0);
        if (ticksToWait > 0)
            await Task.Delay(ticksToWait).ConfigureAwait(false);
        await action().ConfigureAwait(false);
        _ticksPreviousCall = Environment.TickCount;
    }
}
