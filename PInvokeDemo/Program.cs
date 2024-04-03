using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

INPUT ctrl_down = new INPUT();
ctrl_down.type = INPUT_TYPE.INPUT_KEYBOARD;
ctrl_down.Anonymous.ki.wVk = VIRTUAL_KEY.VK_LCONTROL; 

INPUT ctrl_up = new INPUT();
ctrl_up.type = INPUT_TYPE.INPUT_KEYBOARD;
ctrl_up.Anonymous.ki.wVk = VIRTUAL_KEY.VK_LCONTROL;
ctrl_up.Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;

INPUT f_down = new INPUT();
f_down.Anonymous.ki.wVk = VIRTUAL_KEY.VK_F;
f_down.type = INPUT_TYPE.INPUT_KEYBOARD;

INPUT f_up = new INPUT();
f_up.type = INPUT_TYPE.INPUT_KEYBOARD;
f_up.Anonymous.ki.wVk = VIRTUAL_KEY.VK_F;
f_up.Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;

INPUT v_down = new INPUT();
v_down.type = INPUT_TYPE.INPUT_KEYBOARD;
v_down.Anonymous.ki.wVk = VIRTUAL_KEY.VK_V;

INPUT v_up = new INPUT();
v_up.type = INPUT_TYPE.INPUT_KEYBOARD;
v_up.Anonymous.ki.wVk = VIRTUAL_KEY.VK_F;
v_up.Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;

var inputs = new INPUT[] 
{ 
    //ctrl_down, 
    //f_down, 

    //ctrl_up, 
    //f_up, 
    
    ctrl_down,
    v_down,

    ctrl_up,
    v_up,
};

await Task.Delay(TimeSpan.FromSeconds(5));

unsafe
{
    var result = PInvoke.SendInput(inputs, sizeof(INPUT));
    Console.WriteLine(result);
}
