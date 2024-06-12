using System.Buffers;
using System.IO.Pipelines;

namespace PackageDecoder;

public abstract class PackageBase2
{
    private readonly Pipe _pipe;
    protected readonly PipeReader _reader;
    protected readonly PipeWriter _writer;
    public PackageBase2(Pipe pipe)
    {
        _pipe = pipe;
        _reader = pipe.Reader;
        _writer = pipe.Writer;
    }

    protected readonly int InvalidLen = -1;

    public event EventHandler<IEnumerable<byte>>? OnDataParsed;

    /// <summary>
    /// 计算包的长度
    /// </summary>
    protected abstract int CalculatePackageLen(ref ReadOnlySequence<byte> buffer);

    public async Task InputAsync(IEnumerable<byte> bytes)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));

        FlushResult writeResult = await _writer.WriteAsync(bytes.ToArray());
        //_writer.Advance(bytes.Count());
        //await _writer.CompleteAsync();

        while (true)
        {
            ReadResult readResult = await _reader.ReadAsync();
            if (readResult.IsCanceled) break;
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            while (true) 
            {
                if (TryParseLine(ref buffer, out var line))
                {
                    OnDataParsed?.Invoke(this, line.ToArray());
                }
                else
                {
                    break;
                }
            }

            _reader.AdvanceTo(buffer.Start);
            if (readResult.IsCompleted) break;
        }
    }

    private bool TryParseLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line) 
    {
        int packageLen = CalculatePackageLen(ref buffer);
        if (packageLen == InvalidLen || buffer.Length < packageLen)
        {
            line = default;
            return false;
        }
        line = buffer.Slice(0, packageLen);
        buffer = buffer.Slice(packageLen);
        return true;
    }

}
