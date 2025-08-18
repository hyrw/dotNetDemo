using ByteTransport;
using System.Text;

IByteTransport transport = new SerialStreamTransport("COM1", 9600);
transport.Connect();
await transport.SendAsync(Encoding.UTF8.GetBytes("hello"));
var result = await transport.ReceiveAsync();
Console.WriteLine(Encoding.Default.GetString(result));
