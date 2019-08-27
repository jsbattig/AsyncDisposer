using System.Threading;

namespace Ascentis.Framework
{
    public class ConcurrentObjectAccessor<T> where T : new()
    {
        public delegate void LockedExecutionDelegate(T reference);
        public delegate bool GateDelegate(T reference);
        private readonly ReaderWriterLockSlim _refLock;

        public T Reference { get; private set; }

        public ConcurrentObjectAccessor()
        {
            Reference = new T();
            _refLock = new ReaderWriterLockSlim();
        }

        public void ExecuteReadLocked(LockedExecutionDelegate executionDelegate)
        {
            _refLock.EnterReadLock();
            try
            {
                executionDelegate(Reference);
            }
            finally
            {
                _refLock.ExitReadLock();
            }
        }

        public void SwapNewAndExecute(GateDelegate gateOpenDelegate, LockedExecutionDelegate executionDelegate)
        {
            _refLock.EnterUpgradeableReadLock();
            try
            {
                if (!gateOpenDelegate(Reference))
                    return;
                T localReference;
                _refLock.EnterWriteLock();
                try
                {
                    localReference = Reference;
                    Reference = new T();
                }
                finally
                {
                    _refLock.ExitWriteLock();
                }

                executionDelegate(localReference);
            }
            finally
            {
                _refLock.ExitUpgradeableReadLock();
            }
        }
    }
}
