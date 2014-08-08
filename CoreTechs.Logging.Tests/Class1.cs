using System.Linq;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    class Class1
    {
        [Test]
        public void ConcurrentCollectionAlwaysInOrder()
        {
            var range = Enumerable.Range(0, 10000);
            var coll = new ConcurrentList<int>(range);

            CollectionAssert.AreEqual(range,coll);
        }
    }
}
