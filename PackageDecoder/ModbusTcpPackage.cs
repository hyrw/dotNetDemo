using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Channels;
using PackageDecoder.Communication.Modbus;

namespace PackageDecoder;

public class ModbusTcpPackage(PipeReader reader, ChannelWriter<ModbusApplicationDataUnit> channelWriter)
{
    private const int MBAPLength = 7;

    public async Task StartProcessAsync()
    {
        while (true)
        {
            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;
            var packageLen = GetPackageLen(buffer);
            if (packageLen < 1)
            {
                continue;
            }

            var packageSlice = buffer.Slice(0, packageLen);

            var adu = ModbusApplicationDataUnit.FormatTcpAdu(packageSlice.FirstSpan);
            await channelWriter.WriteAsync(adu);

            reader.AdvanceTo(packageSlice.End);

            if (readResult is { IsCompleted: true, Buffer.IsEmpty: true })
            {
                break;
            }
        }
        await reader.CompleteAsync();
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