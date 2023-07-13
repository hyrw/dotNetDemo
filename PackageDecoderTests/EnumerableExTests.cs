using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackageDecoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageDecoder.Tests
{
    [TestClass()]
    public class EnumerableExTests
    {
        [TestMethod()]
        public void StartWithTest()
        {
            var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6 };
            var bytes2 = new byte[] { 1, 2 };
            Assert.IsTrue(bytes1.StartWith(bytes2));
        }
        [TestMethod()]
        public void EndWithTest()
        {
            var bytes1 = new byte[] { 1, 2, 3, 4, 5, 6 };
            var bytes2 = new byte[] {5, 6};
            Assert.IsTrue(bytes1.EndWith(bytes2));
        }
    }
}