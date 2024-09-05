using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Channels;
using PackageDecoder.Communication;

namespace PackageDecoder.Tests;

[TestClass]
public class ModbusTcpPackageTests
{
    private static readonly IPEndPoint TcpEndPoint = new IPEndPoint(IPAddress.Loopback, 502);

    [TestMethod]
    public async Task Test()
    {
        var modbusPdu = ModbusPdu.Create(1, 0, 10);

        var modbusApu = ModbusTcpApu.Create(0, 1, modbusPdu);

        var pipe = new Pipe();
        var writer = pipe.Writer;
        var reader = pipe.Reader;
        var channel = Channel.CreateUnbounded<ModbusTcpApu>();

        var modbusTcpPackage = new ModbusTcpPackage(reader, channel.Writer);

        var sendAndReceiveTask = Task.Run(() =>
        {
            var client = new IocpClient(TcpEndPoint, writer);
            client.Connect();
            while (true)
            {
                client.Send(modbusApu.Encoder());
                client.Receive();
            }
        });

        var processPackageTask = modbusTcpPackage.StartProcessAsync();

        var logApuTask = Task.Run(async () =>
        {
            while (true)
            {
                var apu = await channel.Reader.ReadAsync();
                Debug.WriteLine(apu);
            }
        });

        await Task.WhenAll(sendAndReceiveTask, processPackageTask, logApuTask);

    }
}
