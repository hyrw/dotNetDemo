using ByteTransport;
using System.Text;

IByteTransport comA = new SerialPortTransport("COM1", 9600);
IByteTransport comB = new SerialPortTransport("COM2", 9600);
await comA.ConnectAsync();
await comB.ConnectAsync();

var a = Task.Run(async () =>
{
    byte[] bytes = new byte[1024];
    while (true)
    {
        string msg = "hello";
        await comA.SendAsync(Encoding.Default.GetBytes(msg));
        Memory<byte> buffer = new(bytes);
        await comA.ReceiveAsync(buffer);
        Console.WriteLine($"COMA: {Encoding.Default.GetString(buffer.Span)}");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});
var b = Task.Run(async () =>
{
    byte[] bytes = new byte[1024];
    while (true)
    {

        Memory<byte> buffer = new(bytes);
        await comB.ReceiveAsync(buffer);
        Console.WriteLine($"COMB: {Encoding.Default.GetString(buffer.Span)}");
        string msg = "world hhh  world";
        await comB.SendAsync(Encoding.Default.GetBytes(msg));
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});

await Task.WhenAll(a, b);
