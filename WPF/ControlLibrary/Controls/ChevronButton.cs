using System.Windows;
using System.Windows.Controls;

namespace ControlLibrary.Controls;

public class ChevronButton : Button
{
    static ChevronButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ChevronButton), new FrameworkPropertyMetadata(typeof(ChevronButton)));
    }
}
