using System.Net.Sockets;

namespace ByteTransport;

public class TcpTransport(string host, int port) : IByteTransport
{
    readonly TcpClient client = new();

    public Task ConnectAsync(CancellationToken token = default)
    {
        return client.ConnectAsync(host, port, token).AsTask();
    }

    public Task DisconnectAsync(CancellationToken token = default)
    {
        TaskCompletionSource tcs = new();
        _ = Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            client.Close();
            tcs.SetResult();
        }, token);
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

    public ValueTask SendAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        return SendAsync(new ReadOnlyMemory<byte>(buffer, offset, count), token);
    }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        if (!client.Connected) return ValueTask.FromResult(0);

        var stream = client.GetStream();
        return stream.ReadAsync(buffer, token);
    }

    public ValueTask<int> ReceiveAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        return ReceiveAsync(new Memory<byte>(buffer, offset, count), token);
    }
}
