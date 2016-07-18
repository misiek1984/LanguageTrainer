using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MK.Data.Xml.Tests
{
    [TestClass]
    public class XMLEnumerablePersisterTests
    {
        public class A
        {
            public int Id { get; set; }
        }

        private List<string> _listOfStrings = new List<string> { "1", "abc" };

        private int[] _arrayOfInts = new int[] { 1, 2 };

        private A[] _arrayOfA = new A[] 
        {
            new A { Id = 1 },
            new A { Id = 2 }
        };

        [TestMethod]
        public void SerializeDeserialize_Strings_WorksCorrectly()
        {
            var p = new XMLEnumerablePersister<string>();
            var data = p.Serialize(_listOfStrings);
            var res = p.Deserialize(data).ToArray();
            Assert.IsNotNull(res);
            Assert.AreEqual(_listOfStrings.Count(), res.Count());
            for (var i = 0; i < res.Length; ++i)
                Assert.AreEqual(res[i], _listOfStrings[i]);
        }

        [TestMethod]
        public void SerializeDeserialize_Ints_WorksCorrectly()
        {
            var p = new XMLEnumerablePersister<int>();
            var data = p.Serialize(_arrayOfInts);
            var res = p.Deserialize(data).ToArray();
            for (var i = 0; i < res.Length; ++i)
                Assert.AreEqual(res[i], _arrayOfInts[i]);
        }

        [TestMethod]
        public void DeserializeDeserialize_InstancesOfA_WorksCorrectly()
        {
            var p = new XMLEnumerablePersister<A>();
            var data = p.Serialize(_arrayOfA);
            var res = p.Deserialize(data).ToArray();
            for (var i = 0; i < res.Length; ++i)
                Assert.AreEqual(res[i].Id, _arrayOfA[i].Id);
        }
    }
}
