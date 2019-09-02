using System;
using System.Threading;

namespace Ascentis.Framework
{
    public class ConcurrentObjectAccessor<T>
    {
        public delegate void LockedProcedureDelegate(T reference);
        public delegate object LockedFunctionDelegate(T reference);
        public delegate bool GateDelegate(T reference);

        private readonly object[] _constructorArgs;
        private ReaderWriterLockSlim _refLock;

        public T Reference { get; private set; }

        public ConcurrentObjectAccessor()
        {
            Reference = Activator.CreateInstance<T>();
            InitRefLock();
        }

        public ConcurrentObjectAccessor(params object[] args)
        {
            _constructorArgs = args;
            Reference = (T) Activator.CreateInstance(typeof(T), args);
            InitRefLock();
        }

        private void InitRefLock()
        {
            _refLock = new ReaderWriterLockSlim();
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

        public void ExecuteReadLocked(LockedProcedureDelegate procedureDelegate)
        {
            ExecuteReadLocked(reference =>
            {
                procedureDelegate(reference);
                return null;
            });
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
                    if (_constructorArgs != null)
                        Reference = (T) Activator.CreateInstance(typeof(T), _constructorArgs);
                    else
                        Reference = Activator.CreateInstance<T>();
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
