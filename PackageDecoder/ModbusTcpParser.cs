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

    public async Task<ModbusApplicationDataUnit?> GetOneApu()
    {
        while (true)
        {
            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;
            var consumed = buffer.Start;
            var examined = buffer.End;

            try
            {
                if (TryParseOne(ref buffer, out var frame))
                {
                    consumed = buffer.Start;
                    examined = buffer.End;
                    var adu = ModbusApplicationDataUnit.FormatTcpAdu(frame.FirstSpan);
                    return adu;
                }
                if (readResult is { IsCompleted: true })
                {
                    //if (!readResult.Buffer.IsEmpty) // 剩余的数据不足一个包
                    break;
                }
            }
            finally
            {
                reader.AdvanceTo(consumed, examined);
            }
        }
        return null;
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

    private static int GetPackageLen(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsEmpty ||
            buffer.Length < MBAPLength)
        {
            return 0;
        }

        int dataLen = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(4, 2).FirstSpan);
        return dataLen + MBAPLength - 1;
    }

}