using System;

namespace Mapsui.Fetcher;

public class FetchResult
{
    private readonly IFeature[]? _features;
    private readonly Exception? _exception;

    public FetchResult(IFeature[] features) => _features = features;
    public FetchResult(Exception exception) => _exception = exception;

    public void Handle(Action<IFeature[]> success, Action<Exception> fail)
    {
        if (_features is not null)
            success(_features);
        else if (_exception is not null)
            fail(_exception);
        else
            throw new InvalidOperationException("DataArrivedResult is in an invalid state");
    }
}
