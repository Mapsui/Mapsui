﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Logging;

namespace Mapsui.Fetcher
{
    public class FetchWorker : IDisposable // Todo: Make internal
    {
        private readonly IFetchDispatcher _fetchDispatcher;
        private CancellationTokenSource? _fetchLoopCancellationTokenSource;
        public static long RestartCounter;

        public FetchWorker(IFetchDispatcher fetchDispatcher)
        {
            _fetchDispatcher = fetchDispatcher;
        }

        public void Start()
        {
            if (_fetchLoopCancellationTokenSource == null || _fetchLoopCancellationTokenSource.IsCancellationRequested)
            {
                Interlocked.Increment(ref RestartCounter);
                _fetchLoopCancellationTokenSource?.Dispose();
                _fetchLoopCancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => Fetch(_fetchLoopCancellationTokenSource));
            }
        }

        public void Stop()
        {
            _fetchLoopCancellationTokenSource?.Cancel();
            _fetchLoopCancellationTokenSource?.Dispose();
            _fetchLoopCancellationTokenSource = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fetchLoopCancellationTokenSource?.Dispose();
                _fetchLoopCancellationTokenSource = null;
            }
        }

        private void Fetch(CancellationTokenSource? cancellationTokenSource)
        {
            try
            {
                while (cancellationTokenSource is { Token: { IsCancellationRequested: false } })
                {
                    if (_fetchDispatcher.TryTake(out var method))
                        method();
                    else
                        cancellationTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
            }
        }
    }
}