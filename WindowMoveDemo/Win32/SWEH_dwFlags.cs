namespace OverlayDemo.Win32;

/// <summary>
/// SetWinEventHook() flags
/// </summary>
[Flags]
internal enum SWEH_dwFlags : uint
{
    WINEVENT_OUTOFCONTEXT = 0x0000,     // Events are ASYNC
    WINEVENT_SKIPOWNTHREAD = 0x0001,    // Don't call back for events on installer's thread
    WINEVENT_SKIPOWNPROCESS = 0x0002,   // Don't call back for events on installer's process
    WINEVENT_INCONTEXT = 0x0004         // Events are SYNC, this causes your dll to be injected into every process
}
