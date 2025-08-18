using System.IO.Ports;

namespace ByteTransport;

public class SerialStreamTransport(string com, int baudRate) : IByteTransport
{
    private readonly SerialPort _serialPort = new(com, baudRate);
    private Stream? _stream;

    public void Connect()
    {
        _serialPort.Open();
        _stream = _serialPort.BaseStream;
    }

    public void Disconnect()
    {
        _serialPort.Close();
        _stream = null;
    }

    public bool IsConnected() => _serialPort.IsOpen;

    public async Task<byte[]> ReceiveAsync(CancellationToken token = default)
    {
        if (_stream == null) throw new InvalidOperationException("Not connected");
        using var ms = new MemoryStream();
        var buffer = new byte[1024];
        int bytesRead;

        // https://github.com/microsoft/referencesource/blob/main/System/sys/system/IO/ports/SerialStream.cs#L882
        // while ((bytesRead = await _stream.ReadAsync(buffer, token)) > 0)
        // {
        //     await ms.WriteAsync(buffer.AsMemory(0, bytesRead), token);
        // }

        bytesRead = await _stream.ReadAsync(buffer, token);
        await ms.WriteAsync(buffer.AsMemory(0, bytesRead), token);

        return ms.ToArray();
    }

    public async Task SendAsync(byte[] data, CancellationToken token = default)
    {
        if (_stream == null) throw new InvalidOperationException("Not connected");

        await _stream.WriteAsync(data, token);
    }
}
