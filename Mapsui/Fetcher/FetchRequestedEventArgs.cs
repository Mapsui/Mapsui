using System;

namespace Mapsui.Fetcher;

public class FetchRequestedEventArgs(ChangeType changeType) : EventArgs
{
    public ChangeType ChangeType { get; } = changeType;
}
