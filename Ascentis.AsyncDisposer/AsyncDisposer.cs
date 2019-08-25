using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ascentis.Framework
{
    public class AsyncDisposer
    {
        private static int _disposalInterval = 5000;
        public static int DisposeInterval
        {
            get => _disposalInterval;
            set
            {
                if (value == _disposalInterval)
                    return;
                _disposalInterval = value;
                DisposalTimer.Change(value, value);
            }
        }

        private static readonly Timer DisposalTimer;
        private static ConcurrentQueue<IDisposable> _disposables;
        private static readonly ReaderWriterLockSlim DisposablesLock;

        static AsyncDisposer()
        {
            DisposablesLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _disposables = new ConcurrentQueue<IDisposable>();

            (DisposalTimer = new Timer(state =>
            {
                DisposablesLock.EnterUpgradeableReadLock();
                try
                {
                    if (_disposables.IsEmpty)
                        return;
                    ConcurrentQueue<IDisposable> localQueue;
                    DisposablesLock.EnterWriteLock();
                    try
                    {
                        localQueue = _disposables;
                        _disposables = new ConcurrentQueue<IDisposable>();
                    }
                    finally
                    {
                        DisposablesLock.ExitWriteLock();
                    }
                    while (localQueue.TryDequeue(out var item))
                        item.Dispose();
                }
                finally
                {
                    DisposablesLock.ExitUpgradeableReadLock();
                }
            })).Change(DisposeInterval, DisposeInterval);
        }

        public AsyncDisposer()
        {
            throw new InvalidOperationException("Creating instances of AsyncDisposer() not supported. Class designed to be used as a global singleton");
        }

        public static void Enqueue(IDisposable disposable)
        {
            DisposablesLock.EnterReadLock();
            try
            {
                _disposables.Enqueue(disposable);
            }
            finally
            {
                DisposablesLock.ExitReadLock();
            }
        }
    }
}
