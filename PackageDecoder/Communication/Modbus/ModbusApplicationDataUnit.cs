using System.Buffers.Binary;

namespace PackageDecoder.Communication.Modbus;

public record ModbusApplicationDataUnit(ushort TransactionId, ushort Length, byte UnitId, ModbusPDU Pdu)
{
    public static readonly int MaxTcpPackageSize = 270;
    public ReadOnlySpan<byte> EncoderTcpAdu()
    {
        var packageLen = 7 + Pdu.PackageSize;
        Span<byte> span = new(new byte[packageLen]);
        BinaryPrimitives.WriteUInt16BigEndian(span, TransactionId);
        BinaryPrimitives.WriteUInt16BigEndian(span[2..], 0x00);
        BinaryPrimitives.WriteUInt16BigEndian(span[4..], (ushort)(1 + Pdu.PackageSize)); // UnitId + PduSize
        span[6] = UnitId;

        span[7] = Pdu.FunctionCode;
        Pdu.Data.Span.CopyTo(span[8..]);
        return span;
    }
    public ReadOnlySpan<byte> EncoderRtuAdu()
    {
        const int size = 4; // functionCode + unitId + crc16
        int length = size + Pdu.Data.Length;
        Span<byte> buffer = new(new byte[length]);
        buffer[0] = UnitId;
        Pdu.Data.Span.CopyTo(buffer[1..]);
        return buffer;
    }

    public static ModbusApplicationDataUnit FormatTcpAdu(ReadOnlySpan<byte> frame)
    {
        ushort transactionId = BinaryPrimitives.ReadUInt16BigEndian(frame[..2]);
        ushort protocolFlag = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(2, 2));
        ushort length = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(4,2));
        byte unitId = frame[6];

        if (protocolFlag != 0x00)
        {
            throw new FormatException();
        }

        var pdu = ModbusPDU.Format(frame[7..]);

        return new ModbusApplicationDataUnit(transactionId, length, unitId, pdu);
    }

    public static ModbusApplicationDataUnit FormatRtuAdu(ReadOnlySpan<byte> frame)
    {
        int bufferLen = frame.Length;
        var crc16Span = frame.Slice(bufferLen - 2, 2);

        ushort crc16 = BitConverter.ToUInt16(crc16Span);
        ushort crc16Validate = Crc16Modbus.GetCrc16(crc16Span);
        if (crc16Validate != crc16)
        {
            throw new FormatException();
        }

        byte unitId = frame[0];
        var pdu = ModbusPDU.Format(frame[1..]);

        return new ModbusApplicationDataUnit(default, default, unitId, pdu);
    }
}
