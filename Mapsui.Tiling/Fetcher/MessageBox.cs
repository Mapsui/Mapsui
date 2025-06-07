namespace Mapsui.Tiling.Fetcher;

using System.Threading;

/// <summary>
/// Message box that only preserves the latest message, overwriting what was there before.
/// Locking is used to make sure a message can only be taken once so that it is only processed once.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MessageBox<T> where T : class
{
    private T? _message;

    /// <summary>
    /// Sets the message, replacing any previous one.
    /// </summary>
    public void Put(T message)
    {
        _ = Interlocked.Exchange(ref _message, message);
    }

    /// <summary>
    /// Tries to get the message. If successful, clears the stored message.
    /// </summary>
    /// <param name="message">The message if one was present.</param>
    /// <returns>True if a message was retrieved; false otherwise.</returns>
    public bool TryTake(out T message)
    {
        var temp = Interlocked.Exchange(ref _message, null);
        if (temp != null)
        {
            message = temp;
            return true;
        }

        message = null!;
        return false;
    }
}
