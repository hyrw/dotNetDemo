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
    private const int ModbusTcpApuSize = 260;

    [TestMethod]
    public async Task Test()
    {

        var pipe = new Pipe();
        var writer = pipe.Writer;
        var reader = pipe.Reader;
        var channel = Channel.CreateUnbounded<ModbusTcpApu>();

        var modbusTcpPackage = new ModbusTcpPackage(reader, channel.Writer);

        var sendAndReceiveTask = Task.Run(() =>
        {
            ushort transactionId = 0;
            var modbusPdu = ModbusPdu.Create(1, 0, 10);
            var modbusApu = ModbusTcpApu.Create(transactionId, 1, modbusPdu);
            var client = new IocpClient(TcpEndPoint, writer, ModbusTcpApuSize);
            client.Connect();
            while (true)
            {
                modbusApu.TransactionId = transactionId;
                client.Send(modbusApu.Encoder());
                client.Receive();
                transactionId++;
            }
        });

        var processPackageTask = modbusTcpPackage.StartProcessAsync();

        var logApuTask = Task.Run(async () =>
        {
            while (true)
            {
                var apu = await channel.Reader.ReadAsync();
                if (channel.Reader.Completion.IsCompleted)
                {
                    break;
                }
                // Debug.WriteLine(apu);
            }
        });

        await Task.WhenAll(sendAndReceiveTask, processPackageTask, logApuTask);

    }
}
