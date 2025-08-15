using System.Net.Sockets;

namespace ByteTransportDemo;

public class TcpTransport(string host, int port) : IByteTransport
{
    readonly TcpClient client = new();

    public void Connect()
    {
        client.Connect(host, port);
    }

    public void Disconnect()
    {
        client.Close();
    }

    public bool IsConnected() => client.Connected;

    public async Task<byte[]> ReceiveAsync(CancellationToken token = default)
    {
        if (!client.Connected) return [];
        var stream = client.GetStream();
        var buffer = new byte[4096];
        var bytesRead = await stream.ReadAsync(buffer, token);
        return buffer[..bytesRead];
    }

    public Task SendAsync(byte[] data, CancellationToken token = default)
    {
        if (!client.Connected) return Task.CompletedTask;

        var stream = client.GetStream();
        return stream.WriteAsync(data, 0, data.Length, token);
    }
}
