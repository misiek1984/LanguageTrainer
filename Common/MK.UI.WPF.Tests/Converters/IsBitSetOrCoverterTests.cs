using System;
using System.Globalization;
using MK.UI.WPF.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MK.Utilities.Tests.Converters
{
    [TestClass]
    public class IsBitSetOrCoverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void IsBitSetOrConverter_TargetTypeNotBool_Exception()
        {
            var converter = new IsBitSetOrConverter();
            converter.Convert(null, typeof(int), null, CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void IsBitSetOrConverter_ValueNullParameterNull_False()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(null, typeof (bool), null, CultureInfo.InvariantCulture);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_ValueNullParameterNotNull_False()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(null, typeof(bool), 64, CultureInfo.InvariantCulture);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_ValueNotNullParameterNull_False()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(64, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value64Parameter0_False()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(64, typeof(bool), 0, CultureInfo.InvariantCulture);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value64Parameter64_True()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(64, typeof(bool), 64, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value7Parameter3_True()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(7, typeof(bool), 3, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value7Parameter1_True()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(7, typeof(bool), 1, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value74Parameter5_False()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(74, typeof(bool), 5, CultureInfo.InvariantCulture);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value74ParameterMinus5_True()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(74, typeof(bool), -5, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value74Parameter66_True()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(74, typeof(bool), 66, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value74Parameter67_True()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(74, typeof(bool), 67, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value74Parameter74_True()
        {
            var converter = new IsBitSetAndConverter();
            var res = (bool)converter.Convert(74, typeof(bool), 74, CultureInfo.InvariantCulture);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void IsBitSetOrConverter_Value74ParameterMinus74_False()
        {
            var converter = new IsBitSetOrConverter();
            var res = (bool)converter.Convert(74, typeof(bool), -74, CultureInfo.InvariantCulture);
            Assert.IsFalse(res);
        }
    }
}
