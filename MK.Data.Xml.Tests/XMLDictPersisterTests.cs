using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MK.Data.Xml.Tests
{
    [TestClass]
    public class XMLDictPersisterTests
    {
        public class A
        {
            public int Id { get; set; }
        }

        private Dictionary<string, int> _dict1 = new Dictionary<string, int>
            {
                { "1", 1 },
                { "A", 2 },
            };

        private Dictionary<int, string> _dict2 = new Dictionary<int, string>
            {
                { 1, "1" },
                { 2, "a" },
            };

        private Dictionary<int, A> _dict3 = new Dictionary<int, A>
            {
                { 1, new A { Id = 1 } },
                { 2, new A { Id = 2 } },
            };

        [TestMethod]
        public void SerializeDeserialize_StringsAndInts_WorksCorrectly()
        {
            var p = new XMLDictPersister<string, int>();
            var data = p.Serialize(_dict1);
            var res = p.Deserialize(data);
            Assert.IsNotNull(res);
            Assert.AreEqual(_dict1.Count, res.Count);
            foreach (var kvp in res)
                Assert.AreEqual(kvp.Value, _dict1[kvp.Key]);
        }

        [TestMethod]
        public void SerializeDeserialize_IntsAndStrings_WorksCorrectly()
        {
            var p = new XMLDictPersister<int, string>();
            var data = p.Serialize(_dict2);
            var res = p.Deserialize(data);
            foreach (var kvp in res)
                Assert.AreEqual(kvp.Value, _dict2[kvp.Key]);
        }

        [TestMethod]
        public void DeserializeDeserialize_IntAndAs_WorksCorrectly()
        {
            var p = new XMLDictPersister<int, A>();
            var data = p.Serialize(_dict3);
            var res = p.Deserialize(data);
            foreach (var kvp in res)
                Assert.AreEqual(kvp.Value.Id, _dict3[kvp.Key].Id);
        }
    }
}
