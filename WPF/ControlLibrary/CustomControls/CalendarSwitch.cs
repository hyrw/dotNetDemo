using System.Windows;
using System.Windows.Controls.Primitives;

namespace ControlLibrary.CustomControls;

public class CalendarSwitch : ToggleButton
{
    static CalendarSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarSwitch), new FrameworkPropertyMetadata(typeof(CalendarSwitch)));
    }
}
