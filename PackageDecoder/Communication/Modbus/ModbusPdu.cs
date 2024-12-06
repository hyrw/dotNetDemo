namespace PackageDecoder.Communication.Modbus;

public record ModbusPDU
{
    private readonly byte _functionCode;

    public byte FunctionCode => (byte)(_functionCode & 0b_0111_1111);
    public ReadOnlyMemory<byte> Data { get; }
    public bool HasError => (_functionCode >>> 7) == 1;

    public byte ErrorCode => HasError switch
    {
        true => Data.Span[0],
        false => 0,
    };

    /// <summary>
    /// FunctionCode + Data
    /// </summary>
    public ushort PackageSize => (ushort)(1 + Data.Length);

    private ModbusPDU(byte functionCode, ReadOnlyMemory<byte> data)
    {
        _functionCode = functionCode;
        Data = data;
    }

    public static ModbusPDU Create(byte functionCode, byte[] data)
    {
        return new ModbusPDU(functionCode, data);
    }

    public static ModbusPDU Format(ReadOnlySpan<byte> frame)
    {
        byte functionCode = frame[0];
        byte len = frame[1];
        byte[] data = frame[2..(2 + len)].ToArray();

        return Create(functionCode, data);
    }
}
