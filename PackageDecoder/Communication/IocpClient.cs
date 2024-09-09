using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace PackageDecoder.Communication;

public class IocpClient
{
    private readonly IPEndPoint _remoteEndPoint;
    private readonly Socket _socket;
    private readonly PipeWriter _pipeWriter;
    private readonly SocketAsyncEventArgs _receiveEventArgs = new(), _sendEventArgs = new ();
    
    private readonly AutoResetEvent _connectEvent = new AutoResetEvent(false);  
    private readonly int _bufferSize;

    private TaskCompletionSource _sendTcs;
    private TaskCompletionSource _receiveTcs;

    public bool Connected => _socket?.Connected ?? false;

    public IocpClient(IPEndPoint remoteEndpoint, PipeWriter pipeWriter, int bufferSize)
    {
        this._remoteEndPoint = remoteEndpoint;  
        this._pipeWriter = pipeWriter;
        this._bufferSize = bufferSize;
        _socket = new(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _receiveEventArgs.Completed += OnReceiveCompleted;
        _sendEventArgs.Completed += OnSendCompleted;
    }
    
    public void Connect()
    {
        var socketAsyncEventArgs = new SocketAsyncEventArgs();
        socketAsyncEventArgs.Completed += OnConnectCompleted;
        socketAsyncEventArgs.RemoteEndPoint = this._remoteEndPoint;
        
        if (!_socket.ConnectAsync(socketAsyncEventArgs))
        {
            OnConnectCompleted(_socket, socketAsyncEventArgs);
        }
        _connectEvent.WaitOne();
    }

    public void Disconnect()
    {
        var socketAsyncEventArgs = new SocketAsyncEventArgs();
        _socket.DisconnectAsync(socketAsyncEventArgs);
    }
    private void OnConnectCompleted(object? sender, SocketAsyncEventArgs e)
    {
        _connectEvent.Set();
    }

    public Task SendAsync(Memory<byte> bytes)
    {
        _sendTcs = new TaskCompletionSource();
        _sendEventArgs.SetBuffer(bytes);
        _sendEventArgs.RemoteEndPoint = this._remoteEndPoint;
        if (!_socket.SendAsync(_sendEventArgs))
        {
            OnSendCompleted(_socket, _sendEventArgs);
        }
        return _sendTcs.Task;
    }
    
    private void OnSendCompleted(object? sender, SocketAsyncEventArgs e)
    {
        _sendTcs.SetResult();
    }

    public Task ReceiveAsync()
    {
        _receiveTcs = new TaskCompletionSource();   
        var memory = _pipeWriter.GetMemory(_bufferSize);
        _receiveEventArgs.SetBuffer(memory);
        _receiveEventArgs.RemoteEndPoint = this._remoteEndPoint;
        if (!_socket.ReceiveAsync(_receiveEventArgs))
        {
            OnReceiveCompleted(_socket, _receiveEventArgs);
        }

        return _receiveTcs.Task;
    }
    
    private void OnReceiveCompleted(object? sender, SocketAsyncEventArgs e)
    {
        try
        {
            if (e.SocketError != SocketError.Success ||
                e.BytesTransferred <= 0) return;

            _pipeWriter.Advance(e.BytesTransferred);
            _pipeWriter.FlushAsync();
            _receiveTcs.SetResult();
        }
        catch (Exception ex)
        {
            _receiveTcs.SetException(ex);
        }
    }
}