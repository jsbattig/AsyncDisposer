using System;

namespace UnitTestAsyncDisposer
{
    public class TestClass : IDisposable
    {
        public volatile bool Disposed;
        public void Dispose()
        {
            Disposed = true;
        }
    }
}
