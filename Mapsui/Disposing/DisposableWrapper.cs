using System;

#pragma warning disable IDISP007

namespace Mapsui.Disposing;
public class DisposableWrapper<T>(T wrappedObject, bool ownsObject) : IDisposable
    where T : IDisposable
{
    private readonly T _wrappedObject = wrappedObject ?? throw new ArgumentNullException(nameof(wrappedObject));
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public T WrappedObject => _wrappedObject;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (ownsObject)
                {
                    // Dispose of managed resources
                    _wrappedObject.Dispose();
                }
            }

            _disposed = true;
        }
    }

    // Finalizer (optional, but recommended)
    ~DisposableWrapper()
    {
        Dispose(false);
    }
}
