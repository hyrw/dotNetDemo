using OverlayDemo.ViewModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32;
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

    private const int VK_CONTROL = 0x11;
    #endregion

    #region mouse hook
    private HOOKPROC mouseHookProc;
    private UnhookWindowsHookExSafeHandle mouseHookHandle;
    #endregion

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // todo
        //base.OnClosing(e);

        //if (!hookHandle.IsNull)
        //{
        //    PInvoke.UnhookWinEvent(hookHandle);
        //}
        //if (GCSafetyHandle.IsAllocated)
        //{
        //    GCSafetyHandle.Free();
        //}
        //if (!keyboardHookHandle.IsInvalid && !keyboardHookHandle.IsClosed)
        //{
        //    keyboardHookHandle.Close();
        //}
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        Hide();
    }
}

