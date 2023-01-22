using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

// ignore following warnings
#pragma warning disable VSTHRD101
#pragma warning disable VSTHRD103
#pragma warning disable VSTHRD105
#pragma warning disable VSTHRD110
#pragma warning disable IDISP001
#pragma warning disable IDISP006
#pragma warning disable IDISP007
#pragma warning disable IDISP008
#pragma warning disable IDISP017

namespace Mapsui.Utilities;

public class AsyncLock
{
    private SemaphoreSlim _reentrancy = new SemaphoreSlim(1, 1);
    private int _reentrances = 0;
    // We are using this SemaphoreSlim like a posix condition variable.
    // We only want to wake waiters, one or more of whom will try to obtain
    // a different lock to do their thing. So long as we can guarantee no
    // wakes are missed, the number of awakees is not important.
    // Ideally, this would be "friend" for access only from InnerLock, but
    // whatever.
    internal SemaphoreSlim _retry = new SemaphoreSlim(0, 1);
    private const long UnlockedId = 0x00; // "owning" task id when unlocked
    internal long _owningId = UnlockedId;
    internal int _owningThreadId = (int)UnlockedId;
    private static long AsyncStackCounter = 0;
    // An AsyncLocal<T> is not really the task-based equivalent to a ThreadLocal<T>, in that
    // it does not track the async flow (as the documentation describes) but rather it is
    // associated with a stack snapshot. Mutation of the AsyncLocal in an await call does
    // not change the value observed by the parent when the call returns, so if you want to
    // use it as a persistent async flow identifier, the value needs to be set at the outer-
    // most level and never touched internally.
    private static readonly AsyncLocal<long> _asyncId = new AsyncLocal<long>();
    private static long AsyncId => _asyncId.Value;

#if NETSTANDARD1_3
    private static int ThreadCounter = 0x00;
    private static ThreadLocal<int> LocalThreadId = new ThreadLocal<int>(() => ++ThreadCounter);
    private static int ThreadId => LocalThreadId.Value;
#else
    private static int ThreadId => Thread.CurrentThread.ManagedThreadId;
#endif

    public AsyncLock()
    {
    }

#if !DEBUG
    readonly
#endif
    struct InnerLock : IDisposable
    {
        private readonly AsyncLock _parent;
        private readonly long _oldId;
        private readonly int _oldThreadId;
#if DEBUG
        private bool _disposed;
#endif

        internal InnerLock(AsyncLock parent, long oldId, int oldThreadId)
        {
            _parent = parent;
            _oldId = oldId;
            _oldThreadId = oldThreadId;
#if DEBUG
            _disposed = false;
#endif
        }

        internal async Task<IDisposable> ObtainLockAsync(CancellationToken ct = default)
        {
            while (!await TryEnterAsync(ct))
            {
                // We need to wait for someone to leave the lock before trying again.
                await _parent._retry.WaitAsync(ct);
            }
            // Reset the owning thread id after all await calls have finished, otherwise we
            // could be resumed on a different thread and set an incorrect value.
            _parent._owningThreadId = ThreadId;
            // In case of !synchronous and success, TryEnter() does not release the reentrancy lock
            _parent._reentrancy.Release();
            return this;
        }

        internal async Task<IDisposable?> TryObtainLockAsync(TimeSpan timeout)
        {
            // In case of zero-timeout, don't even wait for protective lock contention
            if (timeout == TimeSpan.Zero)
            {
                if (await TryEnterAsync(timeout))
                {
                    return this;
                }
                return null;
            }

            var now = DateTimeOffset.UtcNow;
            var last = now;
            var remainder = timeout;

            // We need to wait for someone to leave the lock before trying again.
            while (remainder > TimeSpan.Zero)
            {
                if (await TryEnterAsync(remainder))
                {
                    // Reset the owning thread id after all await calls have finished, otherwise we
                    // could be resumed on a different thread and set an incorrect value.
                    _parent._owningThreadId = ThreadId;
                    // In case of !synchronous and success, TryEnter() does not release the reentrancy lock
                    _parent._reentrancy.Release();
                    return this;
                }

                now = DateTimeOffset.UtcNow;
                remainder -= now - last;
                last = now;
                if (remainder < TimeSpan.Zero || !await _parent._retry.WaitAsync(remainder))
                {
                    return null;
                }

                now = DateTimeOffset.UtcNow;
                remainder -= now - last;
                last = now;
            }

            return null;
        }

        internal async Task<IDisposable?> TryObtainLockAsync(CancellationToken cancel)
        {
            try
            {
                while (!await TryEnterAsync(cancel))
                {
                    // We need to wait for someone to leave the lock before trying again.
                    await _parent._retry.WaitAsync(cancel);
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            // Reset the owning thread id after all await calls have finished, otherwise we
            // could be resumed on a different thread and set an incorrect value.
            _parent._owningThreadId = ThreadId;
            // In case of !synchronous and success, TryEnter() does not release the reentrancy lock
            _parent._reentrancy.Release();
            return this;
        }

        internal IDisposable ObtainLock(CancellationToken cancellationToken)
        {
            while (!TryEnter())
            {
                // We need to wait for someone to leave the lock before trying again.
                _parent._retry.Wait(cancellationToken);
            }
            return this;
        }

        internal IDisposable? TryObtainLock(TimeSpan timeout)
        {
            // In case of zero-timeout, don't even wait for protective lock contention
            if (timeout == TimeSpan.Zero)
            {
                if (TryEnter(timeout))
                {
                    return this;
                }
                return null;
            }

            var now = DateTimeOffset.UtcNow;
            var last = now;
            var remainder = timeout;

            // We need to wait for someone to leave the lock before trying again.
            while (remainder > TimeSpan.Zero)
            {
                if (TryEnter(remainder))
                {
                    return this;
                }

                now = DateTimeOffset.UtcNow;
                remainder -= now - last;
                last = now;
                if (!_parent._retry.Wait(remainder))
                {
                    return null;
                }

                now = DateTimeOffset.UtcNow;
                remainder -= now - last;
                last = now;
            }

            return null;
        }

        private async Task<bool> TryEnterAsync(CancellationToken cancel = default)
        {
            await _parent._reentrancy.WaitAsync(cancel);
            return InnerTryEnter();
        }

        private async Task<bool> TryEnterAsync(TimeSpan timeout)
        {
            if (!await _parent._reentrancy.WaitAsync(timeout))
            {
                return false;
            }

            return InnerTryEnter();
        }

        private bool TryEnter()
        {
            _parent._reentrancy.Wait();
            return InnerTryEnter(true /* synchronous */);
        }

        private bool TryEnter(TimeSpan timeout)
        {
            if (!_parent._reentrancy.Wait(timeout))
            {
                return false;
            }
            return InnerTryEnter(true /* synchronous */);
        }

        private bool InnerTryEnter(bool synchronous = false)
        {
            bool result = false;
            try
            {
                if (synchronous)
                {
                    if (_parent._owningThreadId == UnlockedId)
                    {
                        _parent._owningThreadId = ThreadId;
                    }
                    else if (_parent._owningThreadId != ThreadId)
                    {
                        return false;
                    }
                    _parent._owningId = AsyncLock.AsyncId;
                }
                else
                {
                    if (_parent._owningId == UnlockedId)
                    {
                        // Obtain a new async stack ID
                        //_asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);
                        _parent._owningId = AsyncLock.AsyncId;
                    }
                    else if (_parent._owningId != _oldId)
                    {
                        // Another thread currently owns the lock
                        return false;
                    }
                    else
                    {
                        // Nested re-entrance
                        _parent._owningId = AsyncId;
                    }
                }

                // We can go in
                Interlocked.Increment(ref _parent._reentrances);
                result = true;
                return result;
            }
            finally
            {
                // We can't release this in case the lock was obtained because we still need to
                // set the owning thread id, but we may have been called asynchronously in which
                // case we could be currently running on a different thread than the one the
                // locking will ultimately conclude on.
                if (!result || synchronous)
                {
                    _parent._reentrancy.Release();
                }
            }
        }

        public void Dispose()
        {
#if DEBUG
            Debug.Assert(!_disposed);
            _disposed = true;
#endif
            var @this = this;
            var oldId = this._oldId;
            var oldThreadId = this._oldThreadId;
            Task.Run(async () =>
            {
                await @this._parent._reentrancy.WaitAsync();
                try
                {
                    Interlocked.Decrement(ref @this._parent._reentrances);
                    @this._parent._owningId = oldId;
                    @this._parent._owningThreadId = oldThreadId;
                    if (@this._parent._reentrances == 0)
                    {
                        // The owning thread is always the same so long as we
                        // are in a nested stack call. We reset the owning id
                        // only when the lock is fully unlocked.
                        @this._parent._owningId = UnlockedId;
                        @this._parent._owningThreadId = (int)UnlockedId;
                        if (@this._parent._retry.CurrentCount == 0)
                        {
                            @this._parent._retry.Release();
                        }
                    }
                }
                finally
                {
                    @this._parent._reentrancy.Release();
                }
            });
        }
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<IDisposable> LockAsync(CancellationToken ct = default)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);
        return @lock.ObtainLockAsync(ct);
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<bool> TryLockAsync(Action callback, TimeSpan timeout)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);

        return @lock.TryObtainLockAsync(timeout)
            .ContinueWith(state =>
            {
                if (state.Exception is AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                }
                var disposableLock = state.Result;
                if (disposableLock is null)
                {
                    return false;
                }

                try
                {
                    callback();
                }
                finally
                {
                    disposableLock.Dispose();
                }
                return true;
            });
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<bool> TryLockAsync(Func<Task> callback, TimeSpan timeout)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);

        return @lock.TryObtainLockAsync(timeout)
            .ContinueWith(state =>
            {
                if (state.Exception is AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                }
                var disposableLock = state.Result;
                if (disposableLock is null)
                {
                    return Task.FromResult(false);
                }

                return callback()
                    .ContinueWith(result =>
                    {
                        disposableLock.Dispose();

                        if (result.Exception is AggregateException ex)
                        {
                            ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                        }

                        return true;
                    });
            }).Unwrap();
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<bool> TryLockAsync(Action callback, CancellationToken cancel)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);

        return @lock.TryObtainLockAsync(cancel)
            .ContinueWith(state =>
            {
                if (state.Exception is AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                }
                var disposableLock = state.Result;
                if (disposableLock is null)
                {
                    return false;
                }

                try
                {
                    callback();
                }
                finally
                {
                    disposableLock.Dispose();
                }
                return true;
            });
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<bool> TryLockAsync(Func<Task> callback, CancellationToken cancel)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);

        return @lock.TryObtainLockAsync(cancel)
            .ContinueWith(state =>
            {
                if (state.Exception is AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                }
                var disposableLock = state.Result;
                if (disposableLock is null)
                {
                    return Task.FromResult(false);
                }

                return callback()
                    .ContinueWith(result =>
                    {
                        disposableLock.Dispose();

                        if (result.Exception is AggregateException ex)
                        {
                            ExceptionDispatchInfo.Capture(ex.InnerException!).Throw();
                        }

                        return true;
                    });
            }).Unwrap();
    }

    public IDisposable Lock(CancellationToken cancellationToken = default)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        // Increment the async stack counter to prevent a child task from getting
        // the lock at the same time as a child thread.
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);
        return @lock.ObtainLock(cancellationToken);
    }

    public bool TryLock(Action callback, TimeSpan timeout)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        // Increment the async stack counter to prevent a child task from getting
        // the lock at the same time as a child thread.
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);
        var lockDisposable = @lock.TryObtainLock(timeout);
        if (lockDisposable is null)
        {
            return false;
        }

        // Execute the callback then release the lock
        try
        {
            callback();
        }
        finally
        {
            lockDisposable.Dispose();
        }
        return true;
    }

#if TRY_LOCK_OUT_BOOL
    private static readonly NullDisposable NullDisposable = new();

    public IDisposable TryLock(TimeSpan timeout, out bool locked)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        // Increment the async stack counter to prevent a child task from getting
        // the lock at the same time as a child thread.
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);
        var result = @lock.TryObtainLock(timeout);
        locked = result is not null;
        return result ?? NullDisposable;
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<IDisposable> TryLockAsync(CancellationToken ct, out bool locked)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);

        locked = false;

        unsafe
        {
            // This is safe because we are not actually in an async method
            fixed (bool *addr = &locked)
            {
                var addrLong = (ulong)addr;
                return @lock.TryObtainLockAsync(ct).ContinueWith((state) =>
                {
                    var result = state.Result;
                    *(bool*)addrLong = result is not null;
                    return result ?? NullDisposable;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }
    }

    // Make sure InnerLock.LockAsync() does not use await, because an async function triggers a snapshot of
    // the AsyncLocal value.
    public Task<IDisposable> TryLockAsync(TimeSpan timeout, out bool locked)
    {
        var @lock = new InnerLock(this, _asyncId.Value, ThreadId);
        _asyncId.Value = Interlocked.Increment(ref AsyncLock.AsyncStackCounter);

        locked = false;

        unsafe
        {
            // This is safe because we are not actually in an async method
            fixed (bool* addr = &locked)
            {
                var addrLong = (ulong)addr;
                return @lock.TryObtainLockAsync(timeout).ContinueWith((state) =>
                {
                    var result = state.Result;
                    *(bool*)addrLong = result is not null;
                    return result ?? NullDisposable;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }
    }

#endif
}
