using System.IO.Pipelines;
using System.Net;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using PackageDecoder.Communication;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class IocpBenchmark
{
    
    private readonly IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Loopback, 502);
    private readonly byte[] _bytes = [0x00, 0x01,0x00, 0x00,0x00, 0x06, 0x01,0x01, 0x00, 0x02, 0x00, 0x04];
    
    
    [Benchmark]
    [Arguments(10_000)]
    public async Task  TestConnect(int count)
    {
        var pipe = new Pipe();
        IocpClient client = new (_ipEndPoint, pipe.Writer, 1024*10);
        await client.ConnectAsync();
            
        var consumerTask = Task.Factory.StartNew( async () =>
        {
            var loopCount = count;
            var reader = pipe.Reader;
            while (loopCount-- > 0)
            {
                var readResult = await reader.ReadAsync();
                var buffer = readResult.Buffer;
                reader.AdvanceTo(buffer.End);
            }
        }, TaskCreationOptions.LongRunning);
        
        var sendTask = Task.Factory.StartNew( async () =>
        {
            var loopCount = count;
            while (loopCount-- > 0)
            {
                await client.SendAsync(_bytes.AsMemory());
            }
        }, TaskCreationOptions.LongRunning);
        
        var receiveTask = Task.Factory.StartNew( async () =>
        {
            var loopCount = count;  
            while (loopCount-- > 0)
            {
                await client.ReceiveAsync();
            }
        }, TaskCreationOptions.LongRunning);
        
        await Task.WhenAll(sendTask, receiveTask, consumerTask);
    }
}