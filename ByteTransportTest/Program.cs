using ByteTransport;
using System.Text;

IByteTransport comA = new SerialStreamTransport("COM1", 9600);
IByteTransport comB = new SerialStreamTransport("COM2", 9600);
comA.Connect();
comB.Connect();

var a = Task.Run(async () =>
{
    while (true)
    {
        string msg = "hello";
        await comA.SendAsync(Encoding.Default.GetBytes(msg));
        var bytes = await comA.ReceiveAsync();
        Console.WriteLine($"COMA: {Encoding.Default.GetString(bytes)}");
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});
var b = Task.Run(async () =>
{
    while (true)
    {
        var bytes = await comB.ReceiveAsync();
        Console.WriteLine($"COMB: {Encoding.Default.GetString(bytes)}");
        string msg = "world";
        await comB.SendAsync(Encoding.Default.GetBytes(msg));
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
});

await Task.WhenAll(a, b);
