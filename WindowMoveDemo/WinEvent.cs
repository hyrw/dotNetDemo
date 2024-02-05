namespace OverlayDemo
{
    /// <summary>
    /// https://learn.microsoft.com/zh-cn/windows/win32/winauto/event-constants
    /// </summary>
    internal enum WinEvent : uint
    {
        /// <summary>
        /// 窗口的移动或调整大小已完成。
        /// </summary>
        EVENT_SYSTEM_MOVESIZEEND = 0x000B,


        /// <summary>
        /// 对象已更改位置、形状和大小。
        /// </summary>
        EVENT_OBJECT_LOCATIONCHANGE = 0x800B,

    }
}
