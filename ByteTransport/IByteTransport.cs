namespace ByteTransport;

public interface IByteTransport
{
    Task ConnectAsync(CancellationToken token = default);

    Task DisconnectAsync(CancellationToken token = default);

    bool IsConnected { get; }

    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default);

    /// <summary>
    /// 发送字节数组数据
    /// </summary>
    /// <param name="buffer">要发送的字节数组</param>
    /// <param name="offset">数据在数组中的起始位置</param>
    /// <param name="count">要发送的字节数</param>
    /// <param name="token">取消令牌</param>
    ValueTask SendAsync(byte[] buffer, int offset, int count, CancellationToken token = default);

    ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default);

    /// <summary>
    /// 接收数据到字节数组
    /// </summary>
    /// <param name="buffer">接收数据的字节数组</param>
    /// <param name="offset">数据在数组中的起始位置</param>
    /// <param name="count">要接收的最大字节数</param>
    /// <param name="token">取消令牌</param>
    /// <returns>实际接收到的字节数</returns>
    ValueTask<int> ReceiveAsync(byte[] buffer, int offset, int count, CancellationToken token = default);
}

