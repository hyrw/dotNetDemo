using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackageDecoder.Communication.Modbus;

namespace PackageDecoderTests.Communication;

[TestClass]
public class ModbusCommunicationTests
{
    private readonly IPEndPoint _ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 502);
    [TestMethod("Modbus read coil register")]
    public async Task Test()
    {
        var modbus = new ModbusCommunication(_ipEndPoint, ModbusType.TcpIp);

        var readCoilRegister = ReadCoilRegister.Create(0, 1, ModbusFunctionCode.ReadCoil, 0x00, 10);
        var coilRegister = await modbus.ReadCoilRegisterAsync(readCoilRegister);
        
        Assert.AreEqual(readCoilRegister.TransactionId, coilRegister.TransactionId);
        Assert.AreEqual(readCoilRegister.UnitId, coilRegister.UnitId);
        Assert.AreEqual(readCoilRegister.FunctionCode, coilRegister.FunctionCode);
        Assert.AreEqual(readCoilRegister.StartAddress, coilRegister.StartAddress);
        Assert.AreEqual(readCoilRegister.Quantity, coilRegister.Quantity);
        Assert.AreEqual(readCoilRegister.Quantity, coilRegister.Coils.Length);
    }
}