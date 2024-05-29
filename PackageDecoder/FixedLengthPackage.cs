namespace PackageDecoder;

/// <summary>
/// 固定长度包
/// [data(10)+data(10)+...]
/// </summary>
public class FixedLengthPackage : PackageBase
{

    private readonly int PACKAGE_LEN;
    public FixedLengthPackage(int packageLen) : base()
    {
        this.PACKAGE_LEN = packageLen;
    }

    protected override int CalculatePackageLen() => PACKAGE_LEN;
}
