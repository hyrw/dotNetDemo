using System.IO.Ports;

namespace ByteTransportDemo;

public class SerialTransport(string com, int baudRate) : IByteTransport
{
    SerialPort? serialPort;

    public void Connect()
    {
        serialPort ??= new SerialPort(com, baudRate);
        serialPort.Open();
    }

    public void Disconnect()
    {
        if (serialPort is null) return;

        serialPort.Close();
        serialPort.Dispose();
        serialPort = null;
    }

    public bool IsConnected()
    {
        if (serialPort is null) return false;
        return serialPort.IsOpen;
    }

    public async Task<byte[]> ReceiveAsync(CancellationToken token = default)
    {
        if (serialPort is null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not connected");

        token.ThrowIfCancellationRequested();

        var taskSource = new TaskCompletionSource<byte[]>();
        using var registration = token.Register(() => taskSource.TrySetCanceled());

        void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    int bytesToRead = serialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = serialPort.Read(buffer, 0, bytesToRead);
                    taskSource.TrySetResult(bytesRead == bytesToRead ? buffer : buffer[..bytesRead]);
                }
            }
            catch (Exception ex)
            {
                taskSource.TrySetException(ex);
            }
        }

        serialPort.DataReceived += DataReceivedHandler;
        try
        {
            return await taskSource.Task;
        }
        finally
        {
            serialPort.DataReceived -= DataReceivedHandler;
        }
    }

    public Task SendAsync(byte[] data, CancellationToken token = default)
    {
        if (serialPort is null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not connected");
        ArgumentNullException.ThrowIfNull(data);

        token.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            serialPort.Write(data, 0, data.Length);
        }, token);
    }
}
