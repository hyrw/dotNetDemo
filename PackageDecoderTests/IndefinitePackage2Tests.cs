using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Channels;

namespace PackageDecoder.Tests;

[TestClass]
public class IndefinitePackage2Tests
{
    static byte[] _flag = { 0x00, 0x99, 0x00 };
    static int _dataLength = 4;

    [TestMethod]
    public async Task Test()
    {
        var flag = new byte[] { 0, 99, 0 };
        var body = new byte[] { 12, 13, 14, 15, 16 };
        var data = Enumerable.Empty<byte>()
            .Concat(flag)
            .Concat(BitConverter.GetBytes(body.Length))
            .Concat(body)
            .Concat(BitConverter.GetBytes(90));

        var pipe = new Pipe();
        var writer = pipe.Writer;
        var reader = pipe.Reader;

        var channel = Channel.CreateUnbounded<ReadOnlySequence<byte>>();
        _ = await writer.WriteAsync(data.ToArray());

        await ProcessBuffer(reader, channel.Writer);
    }

    private async Task ProcessBuffer(PipeReader reader, ChannelWriter<ReadOnlySequence<byte>> channelWriter)
    {
        byte[] _flag = { 0, 99, 0 };
        int _dataLength = 4;
        int headLength = _flag.Length + _dataLength;

        while (true)
        {
            var readResult = await reader.ReadAsync();
            if (readResult.IsCanceled) { return; }

            var buffer = readResult.Buffer;

            if (buffer.IsEmpty || buffer.Length < _flag.Length)
            {
                // 数据不足
                reader.AdvanceTo(buffer.Start, buffer.End);
                continue;
            }

            var flagSlice = buffer.Slice(0, _flag.Length);
            if (!flagSlice.FirstSpan.StartsWith(_flag))
            {
                // 丢弃
                reader.AdvanceTo(flagSlice.End);
                continue;
            }

            var dataLengthSlice = buffer.Slice(flagSlice.End, _dataLength);
            int dataLength = BitConverter.ToInt32(dataLengthSlice.FirstSpan);
            var dataSlice = buffer.Slice(dataLengthSlice.End);
            if (dataSlice.Length < dataLength)
            {
                // 数据不足
                reader.AdvanceTo(buffer.Start, dataSlice.End);
            }
            dataSlice = dataSlice.Slice(0, dataLength);
            buffer = buffer.Slice(dataSlice.End);

            await channelWriter.WriteAsync(dataSlice);

            // 标记已消费的数据
            reader.AdvanceTo(buffer.Start);

            if (readResult.IsCompleted && buffer.IsEmpty)
            {
                break;
            }
        }

        await reader.CompleteAsync();
    }
}
