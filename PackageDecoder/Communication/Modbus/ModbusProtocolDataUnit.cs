namespace PackageDecoder.Communication.Modbus;

public record ModbusProtocolDataUnit
{
    private readonly byte _functionCode;

    public byte FunctionCode => (byte)(_functionCode & 0b_0111_1111);
    public ReadOnlyMemory<byte> Data { get; }
    public bool HasError => (FunctionCode >>> 7) == 1;
    public ushort PackageSize => (ushort)(1 + Data.Length); // FunctionCode + Data

    private ModbusProtocolDataUnit(byte functionCode, ReadOnlyMemory<byte> data)
    {
        _functionCode = functionCode;
        Data = data;
    }

    public static ModbusProtocolDataUnit Create(byte functionCode, byte[] data)
    {
        return new ModbusProtocolDataUnit(functionCode, data);
    }

    public static ModbusProtocolDataUnit Format(ReadOnlySpan<byte> frame)
    {
        byte functionCode = frame[0];
        byte[] data = frame[1..].ToArray();

        return Create(functionCode, data);
    }
}
