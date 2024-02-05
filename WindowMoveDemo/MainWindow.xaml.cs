using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace OverlayDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    private WINEVENTPROC? hookCallback;
    private GCHandle GCSafetyHandle;
    private Process? targetProc;
    private nint? targetWindowHandle;
    private HWINEVENTHOOK hookHandle;

    public MainWindow()
    {
        InitializeComponent();

        targetProc = Process.GetProcessesByName("PathOfExileSteam").FirstOrDefault();
        targetWindowHandle = targetProc?.MainWindowHandle;

        if (targetProc == null
            || targetWindowHandle == null
            || targetWindowHandle == IntPtr.Zero)
        {
            return;
        }

        hookCallback = OnTargetMoved;
        GCSafetyHandle = GCHandle.Alloc(hookCallback);

        try
        {
            unsafe
            {
                var targetThreadId = PInvoke.GetWindowThreadProcessId((HWND)targetWindowHandle!);

                this.hookHandle = PInvoke.SetWinEventHook(
                    (uint)WinEvent.EVENT_OBJECT_LOCATIONCHANGE,
                    (uint)WinEvent.EVENT_OBJECT_LOCATIONCHANGE,
                    HMODULE.Null,
                    hookCallback,
                    (uint)targetProc!.Id,
                    targetThreadId,
                    (uint)(SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
                    SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
                    SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD)
                    );
            }
        }
        catch
        {
            throw;
        }

    }

    private void OnTargetMoved(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
        Debug.WriteLine($"hook test {0}", hWinEventHook == hookHandle);
        if (hwnd != targetWindowHandle
            && @event == (uint)WinEvent.EVENT_OBJECT_LOCATIONCHANGE
            //&& idObject == (NativeMethods.SWEH_ObjectId)NativeMethods.SWEH_CHILDID_SELF
            )
        {
            return;
        }

        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<Rect>());
        try
        {
            unsafe
            {
                PInvoke.DwmGetWindowAttribute(
                    (HWND)hwnd,
                    Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                    ptr.ToPointer(),
                    (uint)Marshal.SizeOf<Rect>()
                    );
            }
            var rect = Marshal.PtrToStructure<Rect>(ptr);
            SetPosition(rect);
            Topmost = true;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    private void SetPosition(Rect rect)
    {
        Left = rect.Left + 500;
        Top = rect.Top;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (!hookHandle.IsNull)
        {
            PInvoke.UnhookWinEvent(hookHandle);
        }
        if (GCSafetyHandle.IsAllocated)
        {
            GCSafetyHandle.Free();
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        Hide();
    }
}

//SetWinEventHook() flags
[Flags]
public enum SWEH_dwFlags : uint
{
    WINEVENT_OUTOFCONTEXT = 0x0000,     // Events are ASYNC
    WINEVENT_SKIPOWNTHREAD = 0x0001,    // Don't call back for events on installer's thread
    WINEVENT_SKIPOWNPROCESS = 0x0002,   // Don't call back for events on installer's process
    WINEVENT_INCONTEXT = 0x0004         // Events are SYNC, this causes your dll to be injected into every process
}