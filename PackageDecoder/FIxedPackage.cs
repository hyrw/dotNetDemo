using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageDecoder;
public class FixedPackage : PackageBase
{

    private readonly int PACKAGE_LEN;
    public FixedPackage(int packageLen) : base()
    {
        this.PACKAGE_LEN = packageLen;
    }

    protected override int CalculatePackageLen() => PACKAGE_LEN;
}
