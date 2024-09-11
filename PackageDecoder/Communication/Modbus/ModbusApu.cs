using System.Buffers;
using System.Buffers.Binary;

namespace PackageDecoder.Communication.Modbus;

public record ModbusApu(ushort TransactionId, ushort ProtocolFlag, ushort Length, byte UnitId, ModbusPdu Pdu)
{
    public static readonly int MaxTcpPackageSize = 270;
    private static readonly int Crc16Length = sizeof(ushort);
    
    public byte[] EncoderTcpApu()
    {
        var pdu = Pdu.Encoder();
        var pduLength = pdu.Length;
        var packageLen = 7 + pduLength;
        var bytes = new byte[packageLen];
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

    public byte[] EncoderRtuApu()
    {
        var pdu = Pdu.Encoder();
        var pduLength = pdu.Length;
        var packageLen = 1+1+pduLength+Crc16Length;
        var bytes = new byte[packageLen];
        var span = bytes.AsSpan();

        span[0] = UnitId;
        var pduSlice = span[7..];
        for (var i = 0; i < pduLength; i++)
        {
            pduSlice[i] = pdu[i];
        }
        var crc16 = Crc16Modbus.GetCrc16(span[..(packageLen-Crc16Length)]);
        BinaryPrimitives.WriteUInt16BigEndian(span[(packageLen-Crc16Length)..], crc16);
        return bytes;
    }
    public static ModbusApu CreateModbusTcpApu(ushort transactionId, byte unitId, ModbusPdu pdu)
    {
        const ushort length = sizeof(byte) + sizeof(ushort) * 2;
        return new ModbusApu(transactionId, 0x0000, length, unitId, pdu);
    }
    public static ModbusApu DecoderModbusTcpApu(ReadOnlySequence<byte> bufferSlice)
    {
        ushort transactionId = BinaryPrimitives.ReadUInt16BigEndian(bufferSlice.Slice(0, 2).FirstSpan);
        ushort protocolFlag = BinaryPrimitives.ReadUInt16BigEndian(bufferSlice.Slice(2, 2).FirstSpan);
        ushort length = BinaryPrimitives.ReadUInt16BigEndian(bufferSlice.Slice(4, 2).FirstSpan);
        byte unitId = bufferSlice.Slice(6, 1).FirstSpan[0];
        var pdu = ModbusPdu.Decoder(bufferSlice.Slice(7));

        return new ModbusApu(transactionId, protocolFlag, length, unitId, pdu);
    }

    public static ModbusApu CreateModbusRtuApu(byte unitId, ModbusPdu pdu)
    {
        return new ModbusApu(default, default, default, unitId, pdu);
    }

    public static ModbusApu DecoderModbusRtuApu(ReadOnlySequence<byte> bufferSlice)
    {
        byte unitId = bufferSlice.Slice(0, 1).FirstSpan[0];
        var pdu = ModbusPdu.Decoder(bufferSlice.Slice(1));
        return CreateModbusRtuApu(unitId, pdu);
    }
}