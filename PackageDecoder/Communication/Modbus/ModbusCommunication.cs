using System.IO.Pipelines;
using System.Net;

namespace PackageDecoder.Communication.Modbus;

public class ModbusCommunication
{
    private readonly IocpClient? _iocpClient;
    private readonly Pipe _pipe = new();
    private readonly ModbusType _communicationType;
    private readonly ModbusTcpParser modbusParser;

    public bool Connected { get; private set; }
    public ModbusCommunication(IPEndPoint ipEndPoint, ModbusType type)
    {
        this._communicationType = type;
        switch (this._communicationType)
        {
            case ModbusType.TcpIp:
                this._iocpClient = new IocpClient(ipEndPoint, _pipe.Writer, ModbusApplicationDataUnit.MaxTcpPackageSize);
                break;
            case ModbusType.SerialRtu:
            case ModbusType.SerialAscii:
            default: break;
        }
        this.modbusParser = new ModbusTcpParser(_pipe.Reader);
    }

    public async Task ConnectAsync()
    {
        if (this._communicationType == ModbusType.TcpIp)
        {
            await this._iocpClient!.ConnectAsync();
        }
        Connected = true;
    }

    public  Task DisconnectAsync()
    {
        if (this._communicationType == ModbusType.TcpIp)
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

        var requestApu = readCoilRegister.GetModbusAdu();

        await _iocpClient.SendAsync(requestApu.EncoderTcpAdu().ToArray());
        await _iocpClient.ReceiveAsync();
        var responseApu = await this.modbusParser.GetOneApu();

        return readCoilRegister.CreateFromResponse(responseApu!);
    }
}