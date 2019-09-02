using Ascentis.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestAsyncDisposer
{
    [TestClass]
    public class UnitTestConcurrentObjectAccessor
    {
        [TestMethod]
        public void TestCreate()
        {
            var accessor = new ConcurrentObjectAccessor<TestClass>();
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
        }

        [TestMethod]
        public void TestCreateWithName()
        {
            var accessor = new ConcurrentObjectAccessor<TestClass>("Hello");
            Assert.IsNotNull(accessor);
            Assert.IsNotNull(accessor.Reference);
            Assert.AreEqual("Hello", accessor.Reference.Name);
        }
    }
}
