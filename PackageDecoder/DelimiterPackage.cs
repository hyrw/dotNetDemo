namespace PackageDecoder;

public class DelimiterPackage : PackageBase
{

    private readonly IEnumerable<byte> _delimiter = new byte[] { 0x1a, 0x2b };
    public DelimiterPackage() : base() { }

    protected override int CalculatePackageLen()
    {

        if (base._recivedBytes!.Count() < _delimiter.Count()) return -1;

        if (base._recivedBytes!.StartWith(this._delimiter)
            && base._recivedBytes!.EndWith(this._delimiter))
        {
            // todo
        }
        throw new NotImplementedException();
    }
}
