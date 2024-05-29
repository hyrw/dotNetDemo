namespace PackageDecoder;

/// <summary>
/// 结尾分隔符包
/// [data(?)+flag(?)+...]
/// </summary>
public class DelimiterPackage : PackageBase
{

    private readonly IEnumerable<byte> _flag;
    private readonly PatternSearcher<byte> _pattern;
    public DelimiterPackage(IEnumerable<byte> flag) : base()
    {
        _flag = flag;
        _pattern = new PatternSearcher<byte>(flag.ToArray());
    }

    protected override int CalculatePackageLen()
    {
        int index = _pattern.Match(base._receivedBytes.ToArray(), 0, base._receivedBytes.Count());
        if (index < 0)
        {
            return base.InvalidLen;
        }
        else
        {
            return _flag.Count() + index;
        }
    }
}
