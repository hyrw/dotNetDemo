using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackageDecoder.Communication;
using Pipe = System.IO.Pipelines.Pipe;

namespace PackageDecoderTests.Communication;

[TestClass]
public class IocpClientTests
{
    private readonly IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Loopback, 502);
    private readonly byte[] _bytes = [0x00, 0x01,0x00, 0x00,0x00, 0x06, 0x01,0x01, 0x00, 0x02, 0x00, 0x04];

    [TestMethod]
    public async Task  TestConnect()
    {
        var pipe = new Pipe();
        IocpClient client = new (_ipEndPoint, pipe.Writer, 1024*10);
            
        var cts = new TaskCompletionSource();
        
        var consumerTask = Task.Factory.StartNew( async () =>
        {
            var reader = pipe.Reader;

            while (true)
            {
                var readResult = await reader.ReadAsync();
                var buffer = readResult.Buffer;
                reader.AdvanceTo(buffer.End);
            }
        }, TaskCreationOptions.LongRunning);
        
        var sendTask = Task.Factory.StartNew( async () =>
        {
            await client.ConnectAsync();
            while (true)
            {
                await client.SendAsync(_bytes.AsMemory());
            }
        }, TaskCreationOptions.LongRunning);
        
        var receiveTask = Task.Factory.StartNew( async () =>
        {
            await client.ConnectAsync();
            while (true)
            {
                await client.ReceiveAsync();
            }
        }, TaskCreationOptions.LongRunning);
        
        await Task.WhenAll(cts.Task);
    }
}