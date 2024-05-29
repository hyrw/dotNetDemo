namespace PackageDecoder;

public abstract class PackageBase
{
    protected IEnumerable<byte> _receivedBytes;

    protected readonly int InvalidLen = -1;

    public event EventHandler<IEnumerable<byte>>? OnDataParsed;

    /// <summary>
    /// 计算包的长度
    /// </summary>
    protected abstract int CalculatePackageLen();

    public PackageBase()
    {
        _receivedBytes = Enumerable.Empty<byte>();
    }

    public void Input(IEnumerable<byte> bytes)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        _receivedBytes = _receivedBytes.Concat(bytes);

        while (true)
        {
            var packageLen = CalculatePackageLen();
            if (packageLen != InvalidLen && _receivedBytes.Count() >= packageLen)
            {
                OnDataParsed?.Invoke(this, _receivedBytes.Take(packageLen));
                _receivedBytes = _receivedBytes.Skip(packageLen);
            }
            else
            {
                break;
            }
        }
    }
}
