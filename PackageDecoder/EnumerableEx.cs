namespace PackageDecoder;

public static class EnumerableEx
{
    public static bool StartWith(this IEnumerable<byte> first, IEnumerable<byte> second)
    {
        return first.Take(second.Count()).SequenceEqual(second);
    }

    public static bool EndWith(this IEnumerable<byte> first, IEnumerable<byte> second)
    {
        return first.Reverse().Take(second.Count()).SequenceEqual(second.Reverse());
    }
}
