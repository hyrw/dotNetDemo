namespace ByteTransport;

public interface IByteTransport
{
    Task ConnectAsync();
    Task DisconnectAsync();
    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default);
    ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default);
    bool IsConnected { get; }
}

