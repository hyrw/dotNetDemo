using System.Collections;

namespace PackageDecoder.Communication.Modbus;

public record ReadCoilRegister
{
    public ushort TransactionId { get; private init; }
    public byte UnitId { get; private init; }
    public ModbusFunctionCode FunctionCode { get; private init; }
    public ushort StartAddress { get; private init; }
    public ushort Quantity { get; private init; }

    public byte[] Coils { get; private set; } = [];
    
    internal ModbusApu GetModbusTcpApu()
    {
        var pdu = ModbusPdu.Create((byte)FunctionCode, StartAddress, Quantity);
        return ModbusApu.CreateModbusTcpApu(TransactionId, UnitId, pdu);
    }
    
    internal ModbusApu GetModbusRtuApu()
    {
        var pdu = ModbusPdu.Create((byte)FunctionCode, StartAddress, Quantity);
        return ModbusApu.CreateModbusRtuApu(UnitId, pdu);
    }

    public static ReadCoilRegister Create(ushort transactionId, byte unitId, ModbusFunctionCode functionCode,
        ushort startAddress, ushort quantity)
    {
        return new ReadCoilRegister()
        {
            TransactionId = transactionId,
            UnitId = unitId,
            FunctionCode = functionCode,
            StartAddress = startAddress,
            Quantity = quantity,
        };
    }

    public static ReadCoilRegister CreateFromResponse(ushort transactionId, byte unitId, byte functionCode, ushort startAddress, ushort quantity, ReadOnlySpan<byte> buffer)
    {
        var bitArray = new BitArray(buffer.ToArray());
        var coils = new byte[quantity];
        for (var i = 0; i < coils.Length; i++)
        {
            coils[i] = (byte) (bitArray[i] ? 1 : 0);
        }
        return new ReadCoilRegister()
        {
            TransactionId = transactionId,
            UnitId = unitId,
            FunctionCode = (ModbusFunctionCode) functionCode,
            StartAddress = startAddress,
            Quantity = quantity,
            Coils = coils,
        };
    }
}