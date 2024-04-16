using CommunityToolkit.Mvvm.ComponentModel;
using OverlayDemo.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace OverlayDemo.ViewModel;

internal partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    double _left;

    [ObservableProperty]
    double _top;

    [ObservableProperty]
    bool? _topmost;

    [ObservableProperty]
    string? _preseKey;

    [ObservableProperty]
    bool _preseCtrl;

    [ObservableProperty]
    bool _preseShift;

    [ObservableProperty]
    bool _preseAlt;

    [ObservableProperty]
    string? _mousePosition;

    [ObservableProperty]
    bool _fastDiscard;

    Window _window;


    private WINEVENTPROC? hookCallback;
    private GCHandle GCSafetyHandle;
    private Process? targetProc;
    private nint? targetWindowHandle;
    private HWINEVENTHOOK hookHandle;

    #region keyboard hook
    private HOOKPROC keyboardHookProc;
    private UnhookWindowsHookExSafeHandle keyboardHookHandle;

    /// <summary>
    /// Ctrl 不区分左右
    /// </summary>
    private const int VK_CONTROL = 0x11;
    #endregion

    #region mouse hook
    private HOOKPROC mouseHookProc;
    private UnhookWindowsHookExSafeHandle mouseHookHandle;
    #endregion

    public MainWindowViewModel(Window window)
    {
        _window = window;
        keyboardHookProc = LowLevelKeyboardProc;
        mouseHookProc = LowLevelMouseProc;

        var handle = new WindowInteropHelper(window).Handle;

        this.keyboardHookHandle = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_KEYBOARD_LL,
            keyboardHookProc,
            new UnhookWindowsHookExSafeHandle(handle),
            0);
        this.mouseHookHandle = PInvoke.SetWindowsHookEx(
            WINDOWS_HOOK_ID.WH_MOUSE_LL,
            mouseHookProc,
            new UnhookWindowsHookExSafeHandle(handle),
            0);

        targetProc = Process.GetProcessesByName("PathOfExileSteam").FirstOrDefault();
        //targetProc = Process.GetProcessesByName("notepad").FirstOrDefault();
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

    private LRESULT LowLevelMouseProc(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code < 0)
        {
            return PInvoke.CallNextHookEx(keyboardHookHandle, code, wParam, lParam);
        }

        var mouse = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

        switch ((WM)wParam.Value)
        {
            case WM.WM_LBUTTONDOWN:
            case WM.WM_LBUTTONUP:
                Debug.WriteLine($"{mouse.pt}");
                MousePosition = mouse.pt.ToString();
                break;
            default:
                return PInvoke.CallNextHookEx(mouseHookHandle, code, wParam, lParam);
        }
        return (LRESULT)(0);
    }

    private LRESULT LowLevelKeyboardProc(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code < 0)
        {
            return PInvoke.CallNextHookEx(keyboardHookHandle, code, wParam, lParam);
        }

        KBDLLHOOKSTRUCT keyboard = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);


        switch ((WM)wParam.Value)
        {
            case WM.WM_KEYDOWN:
            case WM.WM_SYSKEYDOWN:
            case WM.WM_KEYUP:
            case WM.WM_SYSKEYUP:
                short leftCtrlState = PInvoke.GetAsyncKeyState(VK_CONTROL);
                var preseKey = KeyInterop.KeyFromVirtualKey(keyboard.vkCode);
                PreseKey = preseKey.ToString();
                PreseCtrl = leftCtrlState < 0;
                if (PreseCtrl && preseKey == Key.A && FastDiscard == false)
                {
                    FastDiscard = true;
                }
                else if(PreseCtrl && preseKey == Key.A && FastDiscard == true)
                {
                    FastDiscard = false;
                }
                break;
            default:
                return PInvoke.CallNextHookEx(keyboardHookHandle, code, wParam, lParam);
        }
        return (LRESULT)(0);
    }

    private void OnTargetMoved(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
    {
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
                    (uint)Marshal.SizeOf<RECT>());
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
        //Left = rect.Left + 500;
        //Top = rect.Top;
        _window.Left = rect.Left;
        _window.Top = rect.Top;
    }

}
