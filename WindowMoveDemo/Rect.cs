using System.Runtime.InteropServices;

namespace OverlayDemo;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

}
