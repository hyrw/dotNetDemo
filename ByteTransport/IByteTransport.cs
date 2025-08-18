namespace ByteTransport;

public interface IByteTransport
{
    void Connect();
    void Disconnect();
    Task SendAsync(byte[] data, CancellationToken token = default);
    Task<byte[]> ReceiveAsync(CancellationToken token = default);
    bool IsConnected();
}

