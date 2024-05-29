using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PackageDecoder.Tests;

[TestClass()]
public class DelimiterPackageTests
{
    private readonly byte[] flag = { 0, 255, 0 };
    [TestMethod("整包")]
    public void Test_1()
    {
        var body = new byte[] { 10, 11, 12, 15 };

        var sendPackage = Enumerable.Empty<byte>()
            .Concat(body)
            .Concat(flag);

        PackageBase package = new DelimiterPackage(flag);
        package.OnDataParsed += (_, data) =>
        {
            Assert.IsTrue(sendPackage.SequenceEqual(data));
        };
        package.Input(sendPackage);
    }

    [TestMethod("断包")]
    public void Test_2()
    {
        var body = new byte[] { 10, 11, 12, 15 };

        var sendPackage = Enumerable.Empty<byte>()
            .Concat(body)
            .Concat(flag.Take(2));

        PackageBase package = new DelimiterPackage(flag);
        package.OnDataParsed += (_, data) =>
        {
            Assert.Fail("断包测试失败");
        };
        package.Input(sendPackage);
    }

    [TestMethod("粘包")]
    public void Test_3()
    {
        var body = new byte[] { 10, 11, 12, 15 };

        var package1 = Enumerable.Empty<byte>().Concat(body).Concat(flag);
        var package2 = Enumerable.Empty<byte>().Concat(body.Take(2));

        var sendPackages = Enumerable.Empty<byte>()
            .Concat(package1)
            .Concat(package2);

        var parsedPackage = new List<IEnumerable<byte>>();

        PackageBase package = new DelimiterPackage(flag);
        package.OnDataParsed += (_, data) =>
        {
            parsedPackage.Add(data);
        };
        package.Input(sendPackages);

        if (parsedPackage.Count != 1) Assert.Fail();

        if (!package1.SequenceEqual(parsedPackage.First())) Assert.Fail();

    }

    [TestMethod("两个长度不相同的包")]
    public void Test_4()
    {
        var body = new byte[] { 10, 11, 12, 15 };
        var body2 = new byte[] { 10, 11, 12, 15, 18, 90 };

        var package1 = Enumerable.Empty<byte>().Concat(body).Concat(flag);
        var package2 = Enumerable.Empty<byte>().Concat(body).Concat(flag);

        var sendPackages = Enumerable.Empty<byte>()
            .Concat(package1)
            .Concat(package2);

        var packages = new List<IEnumerable<byte>>()
        {
            package1, package2
        };

        var parsedPackage = new List<IEnumerable<byte>>();

        PackageBase package = new DelimiterPackage(flag);
        package.OnDataParsed += (_, data) =>
        {
            parsedPackage.Add(data);
        };
        package.Input(sendPackages);

        if (parsedPackage.Count != packages.Count) Assert.Fail();

        for (int i = 0; i < packages.Count; i++)
        {
            if (!packages[i].SequenceEqual(parsedPackage[i]))
            {
                Assert.Fail("解析出来的包与原始包不相同");
            }
        }
    }
}