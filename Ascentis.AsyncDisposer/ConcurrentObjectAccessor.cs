using System.Threading;

namespace Ascentis.Framework
{
    public class ConcurrentObjectAccessor<T> where T : new()
    {
        public delegate void LockedProcedureDelegate(T reference);
        public delegate object LockedFunctionDelegate(T reference);
        public delegate bool GateDelegate(T reference);
        private readonly ReaderWriterLockSlim _refLock;

        public T Reference { get; private set; }

        public ConcurrentObjectAccessor()
        {
            Reference = new T();
            _refLock = new ReaderWriterLockSlim();
        }

        public void ExecuteReadLocked(LockedProcedureDelegate procedureDelegate)
        {
            _refLock.EnterReadLock();
            try
            {
                procedureDelegate(Reference);
            }
            finally
            {
                _refLock.ExitReadLock();
            }
        }

        public object ExecuteReadLocked(LockedFunctionDelegate functionDelegate)
        {
            _refLock.EnterReadLock();
            try
            {
                return functionDelegate(Reference);
            }
            finally
            {
                _refLock.ExitReadLock();
            }
        }

        public object SwapNewAndExecute(GateDelegate gateOpenDelegate, LockedFunctionDelegate functionDelegate)
        {
            _refLock.EnterUpgradeableReadLock();
            try
            {
                if (!gateOpenDelegate(Reference))
                    return null;
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

                return functionDelegate(localReference);
            }
            finally
            {
                _refLock.ExitUpgradeableReadLock();
            }
        }

        public void SwapNewAndExecute(GateDelegate gateOpenDelegate, LockedProcedureDelegate procedureDelegate)
        {
            SwapNewAndExecute(gateOpenDelegate, reference =>
            {
                procedureDelegate(reference);
                return null;
            });
        }

        public object SwapNewAndExecute(LockedFunctionDelegate functionDelegate)
        {
            return SwapNewAndExecute(reference => true, functionDelegate);
        }

        public void SwapNewAndExecute(LockedProcedureDelegate procedureDelegate)
        {
            SwapNewAndExecute(reference => true, procedureDelegate);
        }
    }
}
