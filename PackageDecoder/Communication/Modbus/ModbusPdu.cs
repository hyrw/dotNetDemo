using System.Buffers;
using System.Buffers.Binary;

namespace PackageDecoder.Communication.Modbus;

//public record ModbusPdu()
//{
//    public byte FunctionCode { get; init; }

//    #region Request
//    public ushort? StartingAddress { get; set; }
//    public ushort? Quantity { get; set; }
//    #endregion

//    #region Response
//    public byte? ByteCount { get; init; }
//    public byte[]? Data { get; init; }
//    #endregion

//    public static ModbusPdu Create(byte functionCode, ushort startingAddress, ushort quantity)
//    {
//        return new ModbusPdu()
//        {
//            FunctionCode = functionCode,
//            StartingAddress = startingAddress,
//            Quantity = quantity,
//        };
//    }
    
//    public static ModbusPdu Create(ModbusPdu pdu, ReadOnlySequence<byte> buffer)
//    {
//        var decoder = Decoder(buffer);
//        var modbusPdu = new ModbusPdu()
//        {
//            FunctionCode = pdu.FunctionCode,
//            StartingAddress = pdu.StartingAddress,
//            Quantity = pdu.Quantity,
//            ByteCount = decoder.ByteCount,
//            Data = decoder.Data,
//        };
//        return modbusPdu;
//    }
    
//    public byte[] Encoder()
//    {
//        const int size = sizeof(byte) + sizeof(ushort) * 2;
//        var bytes = new byte[size];
//        var span = bytes.AsSpan();
//        span[0] = FunctionCode;
//        BinaryPrimitives.WriteUInt16BigEndian(span[1..], StartingAddress!.Value);
//        BinaryPrimitives.WriteUInt16BigEndian(span[3..], Quantity!.Value);
//        return bytes;
//    }

//    public static ModbusPdu Decoder(ReadOnlySequence<byte> bufferSlice)
//    {
//        var functionCode = bufferSlice.FirstSpan[0];
//        var byteCount = bufferSlice.FirstSpan[1];
//        var data = bufferSlice.Slice(2, byteCount);

//        return new ModbusPdu()
//        {
//            FunctionCode = functionCode,
//            ByteCount = byteCount,
//            Data = data.ToArray(),
//        };
//    }

//}