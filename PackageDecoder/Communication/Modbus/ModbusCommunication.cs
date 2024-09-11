using System.IO.Pipelines;
using System.Net;
using System.Threading.Channels;

namespace PackageDecoder.Communication.Modbus;

public class ModbusCommunication
{
    private readonly IocpClient? _iocpClient;
    private readonly Pipe _pipe = new Pipe();
    private readonly Channel<ModbusApu> channel = Channel.CreateUnbounded<ModbusApu>();
    private readonly ModbusType modbusType;

    public bool Connected { get; private set; }
    public ModbusCommunication(IPEndPoint ipEndPoint, ModbusType type)
    {
        this.modbusType = type;
        switch (this.modbusType)
        {
            case ModbusType.TcpIp:
                this._iocpClient = new IocpClient(ipEndPoint, _pipe.Writer, ModbusApu.MaxTcpPackageSize);
                break;
            case ModbusType.SerialRtu:
            case ModbusType.SerialAscii:
            default: break;
        }
        var modbusPackage = new ModbusTcpPackage(_pipe.Reader, channel.Writer);
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                await modbusPackage.StartProcessAsync();
            }
        }, TaskCreationOptions.LongRunning);
    }

    public async Task ConnectAsync()
    {
        if (this.modbusType == ModbusType.TcpIp)
        {
            await this._iocpClient!.ConnectAsync();
        }
        Connected = true;
    }

    public  Task DisconnectAsync()
    {
        if (this.modbusType == ModbusType.TcpIp)
        {
            _iocpClient!.Disconnect();
        }
        Connected = false;
        return Task.CompletedTask;
    }

    public async Task<ReadCoilRegister> ReadCoilRegisterAsync(ReadCoilRegister readCoilRegister)
    {
        if (!_iocpClient!.Connected)
        {
            await _iocpClient.ConnectAsync();
        }

        var requestApu = readCoilRegister.GetModbusTcpApu();
        
        await _iocpClient.SendAsync(requestApu.EncoderTcpApu());
        await _iocpClient.ReceiveAsync();
        var responseApu = await channel.Reader.ReadAsync();
        
        return ReadCoilRegister.CreateFromResponse(responseApu.TransactionId, responseApu.UnitId, responseApu.Pdu.FunctionCode,
            requestApu.Pdu.StartingAddress!.Value, requestApu.Pdu.Quantity!.Value, responseApu.Pdu.Data);
    }
}