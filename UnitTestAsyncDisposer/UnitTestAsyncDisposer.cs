using System.Threading;
using Ascentis.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestAsyncDisposer
{
    [TestClass]
    public class UnitTestAsyncDisposer
    {
        [TestMethod]
        public void TestEnqueueDisposable()
        {
            var obj = new TestClass();
            AsyncDisposer.DisposeInterval = 500;
            AsyncDisposer.EnqueueForDisposal(obj);
            Thread.Sleep(1000);
            Assert.IsTrue(obj.Disposed);
        }
    }
}
