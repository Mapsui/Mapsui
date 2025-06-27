namespace Mapsui.Fetcher;

using System.Threading;

/// <summary>
/// A bounded mailbox with a capacity of one in which a new message overwrites the existing message.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LatestMailbox<T> where T : class
{
    private T? _message;

    /// <summary>
    /// Sets the message, replacing any previous one.
    /// </summary>
    public void Overwrite(T message)
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
