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
    public class IndefinitePackageTests
    {
        [TestMethod("整包")]
        public void IndefinitePackageTest_1()
        {
            var bytes = new List<byte> ();
            
        }

        [TestMethod("断包")]
        public void IndefinitePackageTest_2()
        {
            Assert.Fail();
        }

        [TestMethod("粘包")]
        public void IndefinitePackageTest_3()
        {
            Assert.Fail();
        }
    }
}