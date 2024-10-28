using System.Buffers.Binary;

namespace PackageDecoder.Communication.Modbus;

public record ReadCoilRegister
{
    public ushort TransactionId { get; private init; }
    public byte UnitId { get; private init; }
    public ModbusFunctionCode FunctionCode { get; private init; }
    public ushort StartAddress { get; private init; }
    public ushort Quantity { get; private init; }

    public byte[] Coils { get; private set; } = [];

    internal ModbusApplicationDataUnit GetModbusAdu()
    {
        const int size = 4;
        var data = new byte[size];
        Span<byte> span = data;
        BinaryPrimitives.WriteUInt16BigEndian(span, StartAddress);
        BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2, 2), Quantity);
        var pdu = ModbusProtocolDataUnit.Create((byte)FunctionCode, data);
        return new ModbusApplicationDataUnit(TransactionId, default, UnitId, pdu);
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

    public ReadCoilRegister CreateFromResponse(ModbusApplicationDataUnit adu)
    {
        var coils = adu.Pdu.ReadCoilRegister(Quantity);
        return new ReadCoilRegister()
        {
            TransactionId = adu.TransactionId,
            UnitId = adu.UnitId,
            FunctionCode = (ModbusFunctionCode) adu.Pdu.FunctionCode,
            StartAddress = StartAddress,
            Quantity = Quantity,
            Coils = coils.ToArray(),
        };
    }
}