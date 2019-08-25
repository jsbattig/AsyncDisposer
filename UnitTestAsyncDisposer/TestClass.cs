using System;
using System.Threading;

namespace UnitTestAsyncDisposer
{
    public class TestClass : IDisposable
    {
        public static volatile int DisposedCount;
        public volatile bool Disposed;
        public void Dispose()
        {
            Disposed = true;
            Interlocked.Increment(ref DisposedCount);
        }
    }
}
