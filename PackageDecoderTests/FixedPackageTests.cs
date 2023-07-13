using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackageDecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PackageDecoder.Tests
{
    [TestClass()]
    public class FixedPackageTests
    {
        [TestMethod("整包")]
        public void FixedPackageTest_1()
        {
            var bytes = new byte[] { 23, 42, 43, 54, 64 };
            PackageBase package = new FixedPackage(5);
            package.OnDataParsed += (_, data) =>
            {
                Assert.AreEqual(bytes, data);
            };
            package.Input(bytes);
        }

        [TestMethod("粘包")]
        public void FixedPackageTest_2()
        {
            var package = new byte[] { 23, 42, 43, 54, 64  };
            var package2 = new byte[] { 23, 42, 43, 54, 64, 5, 4 };
            PackageBase decoder = new FixedPackage(5);
            decoder.OnDataParsed += (_, data) =>
            {
                Assert.IsTrue(package.SequenceEqual(data));
            };
            decoder.Input(package2);
        }

        [TestMethod("断包")]
        public void FixedPackageTest_3()
        {
            var package = new byte[] { 23, 42, 43, 54, 64 };
            var package2 = new byte[] { 23, 42 };
            var package3 = new byte[] { 43, 54 };
            var package4 = new byte[] { 64 };
            PackageBase decoder = new FixedPackage(5);
            decoder.OnDataParsed += (_, data) =>
            {
                Assert.IsTrue(package.SequenceEqual(data));
            };
            decoder.Input(package2);
            decoder.Input(package3);
            decoder.Input(package4);
        }
    }
}