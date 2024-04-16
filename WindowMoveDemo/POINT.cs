using System.Runtime.InteropServices;

namespace OverlayDemo;

[StructLayout(LayoutKind.Sequential)]
struct POINT
{
    long x;
    long y;

    public override string ToString()
    {
        return string.Format($"{x}, {y}");
    }
}
