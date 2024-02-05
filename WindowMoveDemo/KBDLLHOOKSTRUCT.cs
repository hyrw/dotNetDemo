using System.Runtime.InteropServices;

namespace OverlayDemo;

/// <summary>
/// tagKBDLLHOOKSTRUCT
/// </summary>
[StructLayout(LayoutKind.Sequential)]
struct KBDLLHOOKSTRUCT
{
    public int vkCode;
    public int scanCode;
    public int flags;
    public int time;
    public IntPtr dwExtraInfo;
}
