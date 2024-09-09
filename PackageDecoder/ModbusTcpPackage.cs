using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Channels;

namespace PackageDecoder;

public class ModbusTcpPackage(PipeReader reader, ChannelWriter<ModbusTcpApu> channelWriter)
{
    private const int MBAPLength = 7;

    public async Task StartProcessAsync()
    {
        while (true)
        {
            var readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;
            int packageLen = GetPackageLen(buffer);
            if (packageLen < 1)
            {
                continue;
            }

            var packageSlice = buffer.Slice(0, packageLen);

            var modbusApu = ModbusTcpApu.Decoder(packageSlice);
            await channelWriter.WriteAsync(modbusApu);

            reader.AdvanceTo(packageSlice.End);

            if (readResult is { IsCompleted: true, Buffer.IsEmpty: true })
            {
                break;
            }
        }
        await reader.CompleteAsync();
    }

    private int GetPackageLen(ReadOnlySequence<byte> buffer)
    {
        if (buffer.IsEmpty ||
            buffer.Length < MBAPLength)
        {
            return 0;
        }

        int dataLen = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(4, 2).FirstSpan);
        return dataLen + 7 - 1;
    }

}

public record ModbusTcpApu(ushort TransactionId, ushort ProtocolFlag, ushort Length, byte UnitId, ModbusPdu Pdu)
{
    public ushort TransactionId
    {
        get;
        set;
    } = TransactionId;
    
    public byte[] Encoder()
    {
        var pdu = Pdu.Encoder();
        var pduLength = pdu.Length;
        int packageLen = 7 + pduLength;
        byte[] bytes = new byte[packageLen];
        var span = bytes.AsSpan();

        BinaryPrimitives.WriteUInt16BigEndian(span, TransactionId);
        BinaryPrimitives.WriteUInt16BigEndian(span[2..], ProtocolFlag);
        BinaryPrimitives.WriteUInt16BigEndian(span[4..], (ushort)(pduLength + 1));
        span[6] = UnitId;

        var pduSlice = span[7..];
        for (var i = 0; i < pduLength; i++)
        {
            pduSlice[i] = pdu[i];
        }
        return bytes;
    }
    public static ModbusTcpApu Create(ushort transactionId, byte unitId, ModbusPdu pdu)
    {
        const ushort Length = sizeof(byte) + sizeof(ushort) * 2;
        return new ModbusTcpApu(transactionId, 0x0000, Length, unitId, pdu);
    }
    public static ModbusTcpApu Decoder(ReadOnlySequence<byte> bufferSlice)
    {
        ushort transactionId = BinaryPrimitives.ReadUInt16BigEndian(bufferSlice.Slice(0, 2).FirstSpan);
        ushort protocolFlag = BinaryPrimitives.ReadUInt16BigEndian(bufferSlice.Slice(2, 2).FirstSpan);
        ushort length = BinaryPrimitives.ReadUInt16BigEndian(bufferSlice.Slice(4, 2).FirstSpan);
        byte unitId = bufferSlice.Slice(6, 1).FirstSpan[0];
        var pdu = ModbusPdu.Decoder(bufferSlice.Slice(7));

        return new ModbusTcpApu(transactionId, protocolFlag, length, unitId, pdu);
    }
}

public record ModbusPdu()
{
    public byte FunctionCode { get; init; }

    #region Request
    public ushort StartingAddress { get; init; }
    public ushort Quantity { get; init; }
    #endregion

    #region Response
    public byte ByteCount { get; init; }
    public ReadOnlySequence<byte> Data { get; init; }
    #endregion

    public static ModbusPdu Create(byte functionCode, ushort startingAddress, ushort quantity)
    {
        return new ModbusPdu()
        {
            FunctionCode = functionCode,
            StartingAddress = startingAddress,
            Quantity = quantity,
        };
    }
    
    public byte[] Encoder()
    {
        const int Size = sizeof(byte) + sizeof(ushort) * 2;
        var bytes = new byte[Size];
        var span = bytes.AsSpan();
        span[0] = FunctionCode;
        BinaryPrimitives.WriteUInt16BigEndian(span[1..], StartingAddress);
        BinaryPrimitives.WriteUInt16BigEndian(span[3..], Quantity);
        return bytes;
    }

    public static ModbusPdu Decoder(ReadOnlySequence<byte> bufferSlice)
    {
        byte functionCode = bufferSlice.FirstSpan[0];
        byte byteCount = bufferSlice.FirstSpan[1];
        ReadOnlySequence<byte> data = bufferSlice.Slice(2, byteCount);

        return new ModbusPdu()
        {
            FunctionCode = functionCode,
            ByteCount = byteCount,
            Data = data,
        };
    }
}
