using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MK.Utilities.Tests
{
    [TestClass]
    public class DescriptionsProviderTests
    {
        private static string[] CreateIntMap(int count)
        {
            var list= new List<string>();
            for (var i = 0; i < count; ++i)
                list.Add(String.Format("{0}{1}{0}", i.ToString(CultureInfo.InvariantCulture), DescriptionsProvider<int>.DefaultMappingFileDescriptionSeperator));

            return list.ToArray();
        }

        private static string[] CreateDoubleMap(int count)
        {
            var list = new List<string>();
            for (var i = 0; i < count; ++i)
                list.Add(String.Format("{0}{1}{0}", (i + 0.1).ToString(CultureInfo.InvariantCulture), DescriptionsProvider<int>.DefaultMappingFileDescriptionSeperator));

            return list.ToArray();
        }

        [TestMethod]
        public void ParseDoubleMap_0Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<double>();
            map.Parse(CreateDoubleMap(0));

            Assert.IsTrue(map.Descriptions.Count == 0);
        }

        [TestMethod]
        public void ParseDoubleMap_3Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<double>();
            map.Parse(CreateDoubleMap(3));

            Assert.IsTrue(map.Descriptions.Count == 3);
        }

        [TestMethod]
        public void ParseDoubleMap_100lements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<double>();
            map.Parse(CreateDoubleMap(100));

            Assert.IsTrue(map.Descriptions.Count == 100);
        }

        [TestMethod]
        public void ParseIntMap_0Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<double>();
            map.Parse(CreateIntMap(0));

            Assert.IsTrue(map.Descriptions.Count == 0);
        }

        [TestMethod]
        public void ParseIntMap_3Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<double>();
            map.Parse(CreateIntMap(3));

            Assert.IsTrue(map.Descriptions.Count == 3);
        }

        [TestMethod]
        public void ParseIntMap_100Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<double>();
            map.Parse(CreateIntMap(100));

            Assert.IsTrue(map.Descriptions.Count == 100);
        }

        [TestMethod]
        public void ParseStringMap_0Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<string>();
            map.Parse(CreateIntMap(0));

            Assert.IsTrue(map.Descriptions.Count == 0);
        }

        [TestMethod]
        public void ParseStringMap_3Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<string>();
            map.Parse(CreateIntMap(3));

            Assert.IsTrue(map.Descriptions.Count == 3);
        }

        [TestMethod]
        public void ParseStringMap_100Elements_WorksCorrectly()
        {
            var map = new DescriptionsProvider<string>();
            map.Parse(CreateIntMap(100));

            Assert.IsTrue(map.Descriptions.Count == 100);
        }
    }
}
