using System.Windows;
using System.Windows.Controls;

namespace ControlLibrary.Controls;

public class CalendarBoxItem : ListBoxItem
{

    public bool InCurrentMonth
    {
        get { return (bool)GetValue(InCurrentMonthProperty); }
        set { SetValue(InCurrentMonthProperty, value); }
    }

    public DateTime Date { get; set; }

    public static readonly DependencyProperty InCurrentMonthProperty =
        DependencyProperty.Register(nameof(InCurrentMonth), typeof(bool), typeof(CalendarBoxItem), new PropertyMetadata(default));



    static CalendarBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarBoxItem), new FrameworkPropertyMetadata(typeof(CalendarBoxItem)));
    }
}
