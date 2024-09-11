using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Channels;
using PackageDecoder.Communication;
using PackageDecoder.Communication.Modbus;

namespace PackageDecoder.Tests;

[TestClass]
public class ModbusTcpPackageTests
{
    private static readonly IPEndPoint TcpEndPoint = new IPEndPoint(IPAddress.Loopback, 502);
    private const int ModbusTcpApuSize = 260;
    private readonly Pipe pipe = new();
    private readonly Channel<ModbusApu> channel = Channel.CreateUnbounded<ModbusApu>();

    [TestMethod]
    public async Task Test()
    {

        var modbusTcpPackage = new ModbusTcpPackage(pipe.Reader, channel.Writer);

        var sendAndReceiveTask = Task.Run( async () =>
        {
            ushort transactionId = 0;
            var modbusPdu = ModbusPdu.Create(1, 0, 10);
            var client = new IocpClient(TcpEndPoint, pipe.Writer, ModbusTcpApuSize);
            await client.ConnectAsync();
            while (true)
            {
                var modbusApu = ModbusApu.CreateModbusTcpApu(transactionId, 1, modbusPdu);
                await client.SendAsync(modbusApu.EncoderTcpApu());
                await client.ReceiveAsync();
                transactionId++;
            }
        });

        var processPackageTask = modbusTcpPackage.StartProcessAsync();

        var logApuTask = Task.Run(async () =>
        {
            while (true)
            {
                _ = await channel.Reader.ReadAsync();
                if (channel.Reader.Completion.IsCompleted)
                {
                    break;
                }
            }
        });

        await Task.WhenAll(sendAndReceiveTask, processPackageTask, logApuTask);

    }
}
