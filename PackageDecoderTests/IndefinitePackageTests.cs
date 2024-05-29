using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PackageDecoder.Tests;

[TestClass()]
public class IndefinitePackageTests
{
    private readonly byte[] flag = { 0, 255, 0 };

    [TestMethod("整包")]
    public void IndefinitePackageTest_1()
    {
        int dataLength = 4;
        var body = new byte[] { 10, 11, 12, 15 };


        var bytes = Enumerable.Empty<byte>()
            .Concat(flag) // flag
            .Concat(BitConverter.GetBytes(body.Length)) // data_length
            .Concat(body); // data

        PackageBase package = new IndefinitePackage(flag, dataLength);
        package.OnDataParsed += (_, data) =>
        {
            Assert.IsTrue(bytes.SequenceEqual(data));
        };
        package.Input(bytes);
    }

    [TestMethod("断包")]
    public void IndefinitePackageTest_2()
    {
        int dataLength = 4;
        var body = new byte[] { 10, 11, 12, 15 };
        
        var bytes =Enumerable.Empty<byte>()
            .Concat(flag)
            .Concat(BitConverter.GetBytes(body.Length))
            .Concat(body.Take(2));

        PackageBase package = new IndefinitePackage(flag, dataLength);
        package.OnDataParsed += (_, data) =>
        {
            // 因为是断包，解析不出一个整包
            Assert.Fail();
        };
        package.Input(bytes);
    }

    [TestMethod("粘包")]
    public void IndefinitePackageTest_3()
    {
        int dataLength = 4;
        var body = new byte[] { 10, 11, 12, 15 };
        
        var bytes =Enumerable.Empty<byte>()
            .Concat(flag)
            .Concat(BitConverter.GetBytes(body.Length))
            .Concat(body);

        var bytes2 = bytes.Concat(bytes)
            .Concat(flag)
            .Concat(BitConverter.GetBytes(body.Length))
            .Concat(body.Take(2));

        PackageBase package = new IndefinitePackage(flag, dataLength);
        package.OnDataParsed += (_, data) =>
        {
            // 解出来的第一个整包
            Assert.IsTrue(bytes.SequenceEqual(data));
        };
        package.Input(bytes2);
    }

    [TestMethod("两个长度不相同的包")]
    public void IndefinitePackageTest_4()
    {
        int dataLength = 4;
        var body = new byte[] { 10, 11, 12, 15 };
        var body2 = new byte[] { 9, 18 };

        var bytes =Enumerable.Empty<byte>()
            .Concat(flag)
            .Concat(BitConverter.GetBytes(body.Length))
            .Concat(body);

        var bytes2 = Enumerable.Empty<byte>()
            .Concat(flag)
            .Concat(BitConverter.GetBytes(body2.Length))
            .Concat(body2);
        var packages = new List<IEnumerable<byte>>
        {
            bytes,bytes2
        };

        var sendPackages = Enumerable.Empty<byte>().Concat(bytes).Concat(bytes2);

        var parsedPackages = new List<IEnumerable<byte>>();

        PackageBase package = new IndefinitePackage(flag, dataLength);
        package.OnDataParsed += (_, data) =>
        {
            parsedPackages.Add(data);
        };

        package.Input(sendPackages);

        if (packages.Count != parsedPackages.Count) Assert.Fail("解析出来的包数量错误");
        for (int i = 0; i < packages.Count; i++)
        {
            if (!packages[i].SequenceEqual(parsedPackages[i]))
            {
                Assert.Fail("解析出来的包与原始包不相同");
            }
        }
    }
}