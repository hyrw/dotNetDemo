/* using System.IO.Ports;
using System.Threading.Tasks.Sources;

namespace ByteTransport;

public class MyValueTaskSource : IValueTaskSource<short>
{
    ManualResetValueTaskSourceCore<short> core = new();
    public short GetResult(short token) => core.GetResult(token);

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return core.GetStatus(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => core.OnCompleted(continuation, state, token, flags);
}

public class SerialTransport(string com, int baudRate) : IByteTransport
{
    SerialPort? serialPort;

    bool IByteTransport.IsConnected => throw new NotImplementedException();

    public Task ConnectAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        serialPort ??= new SerialPort(com, baudRate);
        serialPort.Open();
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken token = default)
    {
        if (serialPort is null) return Task.CompletedTask;

        token.ThrowIfCancellationRequested();

        serialPort.Close();
        serialPort.Dispose();
        serialPort = null;
        return Task.CompletedTask;
    }

    public bool IsConnected()
    {
        if (serialPort is null) return false;
        return serialPort.IsOpen;
    }

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        if (serialPort is null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not connected");

        token.ThrowIfCancellationRequested();

        var taskSource = new TaskCompletionSource<int>();
        using var registration = token.Register(() => taskSource.TrySetCanceled());

        async void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort!.BytesToRead > 0)
                {
                    int bytesRead = await serialPort.BaseStream.ReadAsync(buffer, token);
                    taskSource.TrySetResult(bytesRead);
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

    public async ValueTask<int> ReceiveAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        if (serialPort is null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not connected");

        token.ThrowIfCancellationRequested();

        var taskSource = new TaskCompletionSource<int>();
        using var registration = token.Register(() => taskSource.TrySetCanceled());

        void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    int bytesRead = serialPort.Read(buffer, offset, count);
                    taskSource.TrySetResult(bytesRead);
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

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
    {
        if (serialPort is null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not connected");
        ArgumentNullException.ThrowIfNull(buffer);

        token.ThrowIfCancellationRequested();
        return serialPort.BaseStream.WriteAsync(buffer, token);
    }

    public async ValueTask SendAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
    {
        if (serialPort is null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not connected");
        ArgumentNullException.ThrowIfNull(buffer);

        token.ThrowIfCancellationRequested();

        await Task.Run(() =>
        {
            token.ThrowIfCancellationRequested();
            serialPort.Write(buffer, offset, count);
        }, token);
    }
} */
