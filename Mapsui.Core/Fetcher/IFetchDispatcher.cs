using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Fetcher
{
    public interface IFetchDispatcher // Todo: Make internal
    {
        bool TryTake([NotNullWhen(true)] out Action? method);
        event PropertyChangedEventHandler PropertyChanged;
    }
}