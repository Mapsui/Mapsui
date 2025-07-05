namespace Mapsui.Fetcher;

using System.Threading;

/// <summary>
/// A thread-safe mailbox that always holds at most one message of type <typeparamref name="T"/>.
/// When a new message is added, it overwrites any existing message.
/// This is useful in scenarios where only the most recent message is relevant,
/// such as UI or viewport updates, and outdated messages should be ignored.
/// </summary>
/// <typeparam name="T">The type of message to store. Must be a reference type.</typeparam>
public class LatestMailbox<T> where T : class
{
    private T? _message;

    public bool IsEmpty => _message is null;

    /// <summary>
    /// Stores the specified message in the mailbox, replacing any previous message.
    /// If a message was already present, it is discarded and replaced by the new one.
    /// </summary>
    /// <param name="message">The message to store. Cannot be null.</param>
    public void Overwrite(T message)
    {
        _ = Interlocked.Exchange(ref _message, message);
    }

    /// <summary>
    /// Attempts to retrieve and remove the current message from the mailbox.
    /// If a message is present, it is returned and the mailbox is cleared.
    /// If no message is present, returns false.
    /// </summary>
    /// <param name="message">
    /// When this method returns, contains the message retrieved from the mailbox,
    /// or null if the mailbox was empty.
    /// </param>
    /// <returns>
    /// True if a message was present and retrieved; false if the mailbox was empty.
    /// </returns>
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
