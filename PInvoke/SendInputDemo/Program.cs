using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

const int MouseEvent_Absolute = 0x8000; 
const int MouserEvent_Hwheel = 0x01000; 
const int MouseEvent_Move = 0x0001; 
const int MouseEvent_Move_noCoalesce = 0x2000;
const int MouseEvent_LeftDown = 0x0002;
const int MouseEvent_LeftUp = 0x0004; 
const int MouseEvent_MiddleDown = 0x0020; 
const int MouseEvent_MiddleUp = 0x0040; 
const int MouseEvent_RightDown = 0x0008; 
const int MouseEvent_RightUp = 0x0010; 
const int MouseEvent_Wheel = 0x0800;
const int MousseEvent_XUp = 0x0100;
const int MousseEvent_XDown = 0x0080;

HOOKPROC mousehookProc =


var mouseHookHandle = PInvoke.SetWindowsHookEx(Windows.Win32.UI.WindowsAndMessaging.WINDOWS_HOOK_ID.WH_MOUSE_LL,
    
    )

List<INPUT> inputs = [];
INPUT mouseLeftButtonDown = new();
mouseLeftButtonDown.type = INPUT_TYPE.INPUT_MOUSE;
mouseLeftButtonDown.Anonymous.mi = new MOUSEINPUT
{
    dwFlags = (MOUSE_EVENT_FLAGS)(MouseEvent_LeftDown | MouseEvent_LeftUp)
};

inputs.Add(mouseLeftButtonDown);

var tcs = new TaskCompletionSource();
var delayTask = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(3));
    PInvoke.SetCursorPos(1090, 560);
    var result = PInvoke.SendInput(inputs.ToArray(), Marshal.SizeOf<INPUT>());
    Console.WriteLine(result);
    tcs.SetResult();
});
await Task.WhenAll(delayTask, tcs.Task);
Console.WriteLine("done!");
