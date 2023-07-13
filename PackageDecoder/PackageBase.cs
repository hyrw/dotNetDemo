namespace PackageDecoder;

public abstract class PackageBase
{
    protected IEnumerable<byte>? _recivedBytes;

    protected readonly int InvalidLen = -1;

    public event EventHandler<IEnumerable<byte>>? OnDataParsed;

    /// <summary>
    /// 计算包的长度
    /// </summary>
    protected abstract int CalculatePackageLen();

    public PackageBase()
    {
        _recivedBytes = Enumerable.Empty<byte>();
    }

    public void Input(IEnumerable<byte> bytes)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        _recivedBytes = _recivedBytes!.Concat(bytes);

        var packageLen = CalculatePackageLen();
        while (_recivedBytes.Count() > packageLen)
        {
            OnDataParsed?.Invoke(this, _recivedBytes.Take(packageLen));
            _recivedBytes = _recivedBytes.Skip(packageLen);
        }
    }
}
