namespace PackageDecoder;
public class IndefinitePackage : PackageBase
{

    private readonly IEnumerable<byte> START_FLAG = new byte[] { 0x1A, 0x2B };
    private const int PACKAGE_LEN = sizeof(int);

    public IndefinitePackage() : base() { }

    protected override int CalculatePackageLen()
    {
        int headLen = this.START_FLAG.Count() + PACKAGE_LEN;
        int len;
        if (base._recivedBytes!.StartWith(this.START_FLAG)
            && base._recivedBytes!.Count() >= headLen)
        {
            var intBytes = base._recivedBytes!.Skip(headLen).Take(PACKAGE_LEN);
            len = BitConverter.ToInt32(intBytes.ToArray());
        }else
        {
            len = base.InvalidLen;
        }
        return len;
    }
}
