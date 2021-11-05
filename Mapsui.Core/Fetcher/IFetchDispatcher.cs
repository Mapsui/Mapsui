using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Layers;

namespace Mapsui.Fetcher
{
    public interface IFetchDispatcher // Todo: Make internal
    {
        bool TryTake([NotNullWhen(true)] ref Action? method);
        void SetViewport(FetchInfo fetchInfo);
        bool Busy { get; }
        event DataChangedEventHandler DataChanged;
        event PropertyChangedEventHandler PropertyChanged;
    }
}