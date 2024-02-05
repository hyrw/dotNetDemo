using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

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

    #region keyboard hook
    private HOOKPROC keyboardHookProc;
    private UnhookWindowsHookExSafeHandle keyboardHookHandle;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    private const int VK_CONTROL = 0x11;
    #endregion

    public MainWindow()
    {
        InitializeComponent();

        keyboardHookProc = LowLevelKeyboardProc;

        var handle = new WindowInteropHelper(this).Handle;

        this.keyboardHookHandle = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL,
            keyboardHookProc,
            new UnhookWindowsHookExSafeHandle(handle),
            0);

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

    private LRESULT LowLevelKeyboardProc(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code < 0)
        {
            return PInvoke.CallNextHookEx(keyboardHookHandle, code, wParam, lParam);
        }

        KBDLLHOOKSTRUCT keyboard = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

        short leftCtrlState = PInvoke.GetAsyncKeyState(VK_CONTROL);

        switch (wParam.Value)
        {
            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
            case WM_KEYUP:
            case WM_SYSKEYUP:
                this.textBlock.Text = KeyInterop.KeyFromVirtualKey(keyboard.vkCode).ToString();
                Debug.WriteLine($"{KeyInterop.KeyFromVirtualKey(keyboard.vkCode)} {Convert.ToString(leftCtrlState, 2)}");
                break;
            default:
                return PInvoke.CallNextHookEx(keyboardHookHandle, code, wParam, lParam);
        }
        return (LRESULT)(0);
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

        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<RECT>());
        try
        {
            unsafe
            {
                PInvoke.DwmGetWindowAttribute(
                    (HWND)hwnd,
                    Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                    ptr.ToPointer(),
                    (uint)Marshal.SizeOf<RECT>()
                    );
            }
            var rect = Marshal.PtrToStructure<RECT>(ptr);
            SetPosition(rect);
            Topmost = true;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    private void SetPosition(RECT rect)
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
        if (!keyboardHookHandle.IsInvalid && !keyboardHookHandle.IsClosed)
        {
            keyboardHookHandle.Close();
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        Hide();
    }

    private void Window_LostFocus(object sender, RoutedEventArgs e)
    {

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