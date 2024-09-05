using System.IO.Pipelines;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackageDecoder.Communication;

namespace PackageDecoder.Tests;

[TestClass]
public class IocpClientTests
{
    private readonly IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Loopback, 502);
    private readonly byte[] _bytes = [0x00, 0x01,0x00, 0x00,0x00, 0x06, 0x01,0x01, 0x00, 0x02, 0x00, 0x04];

    [TestMethod]
    public void TestConnect()
    {
        var pipe = new Pipe();
        IocpClient client = new (_ipEndPoint, pipe.Writer);
        client.Connect();
        client.Send(_bytes.AsMemory());
    }
}