using System.IO.Ports;

namespace ByteTransport;

public class SerialPortTransport(string com, int baudRate) : IByteTransport
{
    readonly SerialPort _serialPort = new(com, baudRate);

    bool IByteTransport.IsConnected => _serialPort.IsOpen;

    public Task ConnectAsync(CancellationToken token = default)
    {
        TaskCompletionSource tcs = new();
        Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            _serialPort.Open();
            tcs.SetResult();
        }, token);
        return tcs.Task;
    }

    public Task DisconnectAsync(CancellationToken token = default)
    {
        TaskCompletionSource tcs = new();
        Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            _serialPort.Close();
            tcs.SetResult();
        }, token);
        return tcs.Task;
    }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        if (!_serialPort.IsOpen) throw new InvalidOperationException("Not connected");

        // https://github.com/microsoft/referencesource/blob/main/System/sys/system/IO/ports/SerialStream.cs#L882
        return _serialPort.BaseStream.ReadAsync(buffer, token);
    }

    public ValueTask<int> ReceiveAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        return ReceiveAsync(new Memory<byte>(buffer, offset, count), token);
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
    {
        if (!_serialPort.IsOpen) throw new InvalidOperationException("Not connected");

        return _serialPort.BaseStream.WriteAsync(buffer, token);
    }

    public ValueTask SendAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        return SendAsync(new ReadOnlyMemory<byte>(buffer, offset, count), token);
    }
}
