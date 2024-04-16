using System.Runtime.InteropServices;

namespace OverlayDemo;

/// <summary>
/// tagKMSLLHOOKSTRUCT
/// </summary>
[StructLayout(LayoutKind.Sequential)]
 struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public int mouseData;
    public int flags;
    public int time;
    public ulong dwExtraInfo;
}
