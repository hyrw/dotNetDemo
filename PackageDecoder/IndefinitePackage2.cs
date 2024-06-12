using System.Buffers;
using System.IO.Pipelines;

namespace PackageDecoder;

/// <summary>
/// [flag(?)+data_length(?)+data+...]
/// </summary>
public class IndefinitePackage2 : PackageBase2
{

    private readonly byte[] _flag;
    private readonly int _flagLength;
    private readonly int _dataLength;

    public IndefinitePackage2(Pipe pipe, byte[] flag, int dataLength) : base(pipe)
    {
        _flag = flag.ToArray();
        _flagLength = _flag.Length;
        _dataLength = dataLength;
    }

    protected override int CalculatePackageLen(ref ReadOnlySequence<byte> buffer)
    {
        int headLength = _flagLength + _dataLength;

        if (buffer.Length < headLength) return base.InvalidLen;

        var flagSlice = buffer.Slice(0, _flagLength);

        if (!flagSlice.FirstSpan.StartsWith(_flag)) 
        {
            buffer = buffer.Slice(_flagLength);
            return base.InvalidLen; 
        }
        int dataLength = BitConverter.ToInt32(buffer.Slice(_flagLength, _dataLength).FirstSpan);

        return headLength + dataLength;
    }
}
