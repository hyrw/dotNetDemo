using System.Windows.Input;

namespace OverlayDemo.Extension;

internal static class KeyExtension
{
    public static int GetVirtualKey(this Key key)
    {
        return KeyInterop.VirtualKeyFromKey(key);
    }
}
