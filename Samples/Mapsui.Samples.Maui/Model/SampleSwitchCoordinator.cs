// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

#pragma warning disable IDE0005 // Also compiled by the test project, which does not enable implicit usings.
using System;
using System.Threading;
using System.Threading.Tasks;
#pragma warning restore IDE0005

namespace Mapsui.Samples.Maui.Model;

// Called on the UI context. A newer selection cancels publication of an older result,
// even when the sample's CreateMapAsync does not itself accept cancellation.
internal sealed class SampleSwitchCoordinator : IDisposable
{
    private Task _pending = Task.CompletedTask;
    private Action? _cancelPending;
    private int _version;
    private bool _disposed;

    public Task RunAsync(Func<CancellationToken, Task> action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _cancelPending?.Invoke();
        var cancellation = new CancellationTokenSource();
        _cancelPending = cancellation.Cancel;
        var version = ++_version;
        return _pending = ExecuteAsync(_pending, cancellation, version, action);
    }

    private async Task ExecuteAsync(Task previous, CancellationTokenSource cancellation, int version, Func<CancellationToken, Task> action)
    {
#pragma warning disable IDISP007 // This private method owns the CTS created by RunAsync for this request.
        using (cancellation)
#pragma warning restore IDISP007
        {
            try
            {
                // The caller of the failed request reports its error; it must not prevent the next selection.
#pragma warning disable VSTHRD003 // Previous is our own asynchronous UI request; no synchronous waits are used.
                try { await previous; }
#pragma warning restore VSTHRD003
                catch (Exception) { }
                cancellation.Token.ThrowIfCancellationRequested();
                await action(cancellation.Token);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            finally
            {
                if (version == _version)
                    _cancelPending = null;
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _cancelPending?.Invoke();
        _cancelPending = null;
    }
}
