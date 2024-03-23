using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public interface IFetchDispatcher // Todo: Make internal
{
    bool TryTake([NotNullWhen(true)] out Func<CancellationToken, Task>? method);
    event PropertyChangedEventHandler PropertyChanged;
}
