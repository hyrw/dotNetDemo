using System.IO.Ports;

namespace ByteTransport;

public class SerialPortTransport(string com, int baudRate) : IByteTransport
{
    readonly SerialPort _serialPort = new(com, baudRate);

    bool IByteTransport.IsConnected => _serialPort.IsOpen;

    public Task ConnectAsync()
    {
        TaskCompletionSource tcs = new();
        Task.Run(() =>
        {
            _serialPort.Open();
            tcs.SetResult();
        });
        return tcs.Task;
    }

    public Task DisconnectAsync()
    {
        TaskCompletionSource tcs = new();
        Task.Run(() =>
        {
            _serialPort.Close();
            tcs.SetResult();
        });
        return tcs.Task;
    }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        if (!_serialPort.IsOpen) throw new InvalidOperationException("Not connected");

        // https://github.com/microsoft/referencesource/blob/main/System/sys/system/IO/ports/SerialStream.cs#L882
        return _serialPort.BaseStream.ReadAsync(buffer, token);
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
    {
        if (!_serialPort.IsOpen) throw new InvalidOperationException("Not connected");

        return _serialPort.BaseStream.WriteAsync(buffer, token);
    }
}
