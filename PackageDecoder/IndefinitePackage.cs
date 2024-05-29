namespace PackageDecoder;

/// <summary>
/// [flag(?)+data_length(?)+data+...]
/// </summary>
public class IndefinitePackage : PackageBase
{

    private readonly IEnumerable<byte> _flag;
    private readonly int _dataLength;

    public IndefinitePackage(IEnumerable<byte> flag, int dataLength) : base()
    {
        _flag = flag.ToArray();
        _dataLength = dataLength;
    }

    protected override int CalculatePackageLen()
    {
        int headLength = _flag.Count() + _dataLength;
        int packageLength;
        if (base._receivedBytes.StartWith(this._flag)
            && base._receivedBytes.Count() >= headLength)
        {
            int dataLength = BitConverter.ToInt32(
                base._receivedBytes.Skip(this._flag.Count()).Take(_dataLength).ToArray());
            packageLength = headLength + dataLength;
        }
        else
        {
            packageLength = base.InvalidLen;
        }
        return packageLength;
    }
}
