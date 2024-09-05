using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackageDecoder.Communication;
using Pipe = System.IO.Pipelines.Pipe;

namespace PackageDecoder.Tests;

[TestClass]
public class IocpClientTests
{
    private readonly IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Loopback, 502);
    private readonly byte[] _bytes = [0x00, 0x01,0x00, 0x00,0x00, 0x06, 0x01,0x01, 0x00, 0x02, 0x00, 0x04];

    [TestMethod]
    public async Task  TestConnect()
    {
        var pipe = new Pipe();
        var cts = new TaskCompletionSource();
        _ = Task.Run(()=>
        {
            Console.ReadKey();
            cts.SetResult();
        });

        var consumerTask = Task.Run( async () =>
        {
            var reader = pipe.Reader;

            while (true)
            {
                var readResult = await reader.ReadAsync();
                var buffer = readResult.Buffer;
                reader.AdvanceTo(buffer.End);
                
                if (readResult.IsCompleted && buffer.IsEmpty) break;
            }
            reader.Complete();
        });
        
        var task = Task.Run( async () =>
        {
            var writer = pipe.Writer;
            IocpClient client = new (_ipEndPoint, writer, 1024*10);
            client.Connect();
            while (true)
            {
                client.Send(_bytes.AsMemory());
                client.Receive();
                if (cts.Task.IsCompleted) break;
            }
            await writer.CompleteAsync();
        });
        
        await Task.WhenAll(task, cts.Task, consumerTask);
    }
}