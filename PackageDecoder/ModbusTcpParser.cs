using PackageDecoder.Communication.Modbus;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;

namespace PackageDecoder;

/// <summary>
/// Modbus Tcp Protocol Parser
/// </summary>
/// <param name="reader"></param>
public class ModbusTcpParser(PipeReader reader)
{
    private const int MBAPLength = 7;

    public async Task<ModbusApplicationDataUnit?> Parser()
    {
        var readResult = await reader.ReadAsync();
        var buffer = readResult.Buffer;
        var consumed = buffer.Start;
        var examined = buffer.End;

        ModbusApplicationDataUnit? adu = default;

        try
        {
            if (TryParseOne(ref buffer, out var frame))
            {
                consumed = buffer.Start;
                examined = buffer.End;
                adu = ModbusApplicationDataUnit.FormatTcpAdu(frame.FirstSpan);
            }
        }
        finally
        {
            reader.AdvanceTo(consumed, examined);
        }
        return adu;
    }

    static bool TryParseOne(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> frame)
    {
        frame = default;
        int len = GetPackageLen(buffer);
        if (len > 0)
        {
            frame = buffer.Slice(0, len);
            buffer = buffer.Slice(len);
            return true;
        }
        return false;
    }

    static int GetPackageLen(ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length < MBAPLength) return 0;

        int dataLen = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(4, 2).FirstSpan);
        return dataLen + MBAPLength - 1;
    }

}