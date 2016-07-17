using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MK.MyMath.Tests
{
    [TestClass]
    public class MyMathTests
    {
        [TestMethod]
        public void PickSomeInRandomOrderWithWeights_MaxCountEqualToItemsCount_WorksCorrectly()
        {
            var data = GetData(100);
            var res = MyMath.PickSomeInRandomOrderWithWeights(data, 100);
            Assert.AreEqual(100, res.Count);
            Assert.IsTrue(res.All(i => data.Any(t => t.Item2 == i)));
        }

        [TestMethod]
        public void PickSomeInRandomOrderWithWeights_MaxCountLessThanItemsCount_WorksCorrectly()
        {
            var data = GetData(100);
            var res = MyMath.PickSomeInRandomOrderWithWeights(data, 10);
            Assert.AreEqual(10, res.Count);
            Assert.IsTrue(res.All(i => data.Any(t => t.Item2 == i)));
        }

        [TestMethod]
        public void PickSomeInRandomOrderWithWeights_MaxCountGreaterThanThanItemsCount_WorksCorrectly()
        {
            var data = GetData(100);
            var res = MyMath.PickSomeInRandomOrderWithWeights(data, 1000);
            Assert.AreEqual(100, res.Count);
            Assert.IsTrue(res.All(i => data.Any(t => t.Item2 == i)));
        }

        private List<Tuple<double, int>> GetData(int max)
        {
            return Enumerable.Range(0, max).Select(i => new Tuple<double, int>(i, i)).ToList();
        }



        [TestMethod]
        public void LevenshteinDistance_FirstVectorIsEmpty_4()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { }, new List<int> { 1, 2, 3, 4 });
            Assert.AreEqual(res, 4);

            var res2 = MyMath.LevenshteinDistance(new List<char> { }, new List<char> { '1', '2', '3', '4' });
            Assert.AreEqual(res2, 4);
        }

        [TestMethod]
        public void LevenshteinDistance_SecondVectorIsEmpty_4()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4 }, new List<int> { });
            Assert.AreEqual(res, 4);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4' }, new List<char> { });
            Assert.AreEqual(res2, 4);
        }

        [TestMethod]
        public void LevenshteinDistance_2IdenticalVectors_0()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4 } , new List<int> { 1, 2, 3, 4 });
            Assert.AreEqual(res, 0);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4' }, new List<char> { '1', '2', '3', '4' });
            Assert.AreEqual(res2, 0);
        }

        [TestMethod]
        public void LevenshteinDistance_SecondVectorIsInversionOfTheFirst_4()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4 }, new List<int> { 4, 3, 2, 1 });
            Assert.AreEqual(res, 4);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4' }, new List<char> { '4', '3', '2', '1' });
            Assert.AreEqual(res2, 4);
        }

        [TestMethod]
        public void LevenshteinDistance_FirstAndLastItemAreDifferent_2()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4 }, new List<int> { 4, 2, 3, 1 });
            Assert.AreEqual(res, 2);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4' }, new List<char> { '4', '2', '3', '1' });
            Assert.AreEqual(res2, 2);
        }

        [TestMethod]
        public void LevenshteinDistance_SecondVectorIsLongerThanTheFirstOneButTheBeginningIsTheSame_3()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4 }, new List<int> { 1, 2, 3, 4, 5, 6, 7 });
            Assert.AreEqual(res, 3);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4' }, new List<char> { '1', '2', '3', '4', '5', '6', '7' });
            Assert.AreEqual(res2, 3);
        }

        [TestMethod]
        public void LevenshteinDistance_FirstVectorIsLongerThanTheFirstOneButTheBeginningIsTheSame_3()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4, 5, 6, 7 }, new List<int> { 1, 2, 3, 4 });
            Assert.AreEqual(res, 3);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4', '5', '6', '7' }, new List<char> { '1', '2', '3', '4' });
            Assert.AreEqual(res2, 3);
        }

        [TestMethod]
        public void LevenshteinDistance_FirstVectorIsLongerThanTheFirstOneButTheEndIsTheSame_3()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 5, 6, 7, 1, 2, 3, 4 }, new List<int> { 1, 2, 3, 4 });
            Assert.AreEqual(res, 3);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '5', '6', '7', '1', '2', '3', '4' }, new List<char> { '1', '2', '3', '4' });
            Assert.AreEqual(res2, 3);
        }

        [TestMethod]
        public void LevenshteinDistance_OneItemInTheMiddleIsDifferent_1()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 5, 6, 7, 1, 2, 3, 4 }, new List<int> { 5, 6, 7, 9, 2, 3, 4 });
            Assert.AreEqual(res, 1);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '5', '6', '7', '1', '2', '3', '4' }, new List<char> { '5', '6', '7', '9', '2', '3', '4' });
            Assert.AreEqual(res2, 1);
        }

        [TestMethod]
        public void LevenshteinDistance_Something_2()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 1, 2, 3, 4, 5 }, new List<int> { 2, 3, 4, 5, 3  });
            Assert.AreEqual(res, 2);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '1', '2', '3', '4', '5' }, new List<char> { '2', '3', '4', '5', '3'  });
            Assert.AreEqual(res2, 2);
        }

        [TestMethod]
        public void LevenshteinDistance_Something_3()
        {
            var res = MyMath.LevenshteinDistance(new List<int> { 9, 2, 3, 4, 7 }, new List<int> { 2, 3, 4, 5, 3 });
            Assert.AreEqual(res, 3);

            var res2 = MyMath.LevenshteinDistance(new List<char> { '9', '2', '3', '4', '7' }, new List<char> { '9', '3', '4', '5', '3' });
            Assert.AreEqual(res2, 3);
        }
    }
}
