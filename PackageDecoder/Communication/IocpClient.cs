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
    private readonly AutoResetEvent _sendEvent = new AutoResetEvent(false);  
    private readonly AutoResetEvent _receiveEvent = new AutoResetEvent(false);  
    
    public bool Connected => _socket?.Connected ?? false;

    public IocpClient(IPEndPoint remoteEndpoint, PipeWriter pipeWriter)
    {
        this._remoteEndPoint = remoteEndpoint;  
        this._pipeWriter = pipeWriter;
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

    public void Send(Memory<byte> bytes) 
    {
        _sendEventArgs.SetBuffer(bytes);
        _sendEventArgs.RemoteEndPoint = this._remoteEndPoint;
        if (!_socket.SendAsync(_sendEventArgs))
        {
            OnSendCompleted(_socket, _sendEventArgs);
        }
        
        _sendEvent.WaitOne();
    }
    
    private void OnSendCompleted(object? sender, SocketAsyncEventArgs e)
    {
        _sendEvent.Set();
        if (e.SocketError != SocketError.Success) return;
    }

    public void Receive()
    {
        _receiveEventArgs.RemoteEndPoint = this._remoteEndPoint;
        if (!_socket.ReceiveAsync(_receiveEventArgs))
        {
            OnReceiveCompleted(_socket, _receiveEventArgs);
        }
        _receiveEvent.WaitOne();
    }
    private void OnReceiveCompleted(object? sender, SocketAsyncEventArgs e)
    {
        _receiveEvent.Set();
        if (e.SocketError != SocketError.Success) return;
        
        if (e.BytesTransferred <= 0) return;
        _pipeWriter.Advance(e.BytesTransferred);
        _pipeWriter.FlushAsync();
    }
}