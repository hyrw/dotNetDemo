using System.Net.Sockets;

namespace ByteTransport;

public class TcpTransport(string host, int port) : IByteTransport
{
    readonly TcpClient client = new();

    public Task ConnectAsync()
    {
        return client.ConnectAsync(host, port);
    }

    public Task DisconnectAsync()
    {
        TaskCompletionSource tcs = new();
        Task.Run(() =>
        {
            client.Close();
            tcs.SetResult();
        });
        return tcs.Task;
    }

    public bool IsConnected => client.Connected;

    public async Task<byte[]> ReceiveAsync(CancellationToken token = default)
    {
        if (!client.Connected) return [];
        var stream = client.GetStream();
        var buffer = new byte[4096];
        var bytesRead = await stream.ReadAsync(buffer, token);
        return buffer[..bytesRead];
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
    {
        if (!client.Connected) return ValueTask.CompletedTask;

        var stream = client.GetStream();
        return stream.WriteAsync(buffer, token);
    }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        if (!client.Connected) return ValueTask.FromResult(0);

        var stream = client.GetStream();
        return stream.ReadAsync(buffer, token);
    }
}
