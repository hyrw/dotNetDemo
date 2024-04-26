using System.Windows;
using System.Windows.Controls;

namespace ControlLibrary.CustomControls;

public class CalendarBox : ListBox
{
    static CalendarBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarBox), new FrameworkPropertyMetadata(typeof(CalendarBox)));
    }
}
