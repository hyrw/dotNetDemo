using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ControlLibrary.Controls;

[TemplatePart(Name = PART_CalendarSwitch, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_CalendarBox, Type = typeof(ListBox))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
[TemplatePart(Name = PART_Left, Type = typeof(Button))]
[TemplatePart(Name = PART_Right, Type = typeof(Button))]
public class SmartDate : Control
{
    #region PART
    const string PART_CalendarSwitch = "PART_CalendarSwitch";
    const string PART_CalendarBox = "PART_CalendarBox";
    const string PART_Popup = "PART_Popup";
    const string PART_Left = "PART_Left";
    const string PART_Right = "PART_Right";
    CalendarSwitch? _switch;
    CalendarBox? _box;
    Popup? _popup;
    #endregion

    public DateTime? SelectedDate
    {
        get { return (DateTime?)GetValue(SelectedDateProperty); }
        set { SetValue(SelectedDateProperty, value); }
    }
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime?), typeof(SmartDate), new PropertyMetadata(default));


    public DateTime? CurrentMonth
    {
        get { return (DateTime?)GetValue(CurrentMonthProperty); }
        set { SetValue(CurrentMonthProperty, value); }
    }
    public static readonly DependencyProperty CurrentMonthProperty =
        DependencyProperty.Register(nameof(CurrentMonth), typeof(DateTime?), typeof(SmartDate), new PropertyMetadata(default));



    public bool? KeepPopupOpen
    {
        get { return (bool?)GetValue(KeepPopupOpenProperty); }
        set { SetValue(KeepPopupOpenProperty, value); }
    }
    public static readonly DependencyProperty KeepPopupOpenProperty =
        DependencyProperty.Register(nameof(KeepPopupOpen), typeof(bool?), typeof(SmartDate), new PropertyMetadata(default));

    static SmartDate()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SmartDate), new FrameworkPropertyMetadata(typeof(SmartDate)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _switch = (CalendarSwitch)GetTemplateChild(PART_CalendarSwitch);
        _box = (CalendarBox)GetTemplateChild(PART_CalendarBox); ;
        _popup = (Popup)GetTemplateChild(PART_Popup);
        Button leftButton = (Button)GetTemplateChild(PART_Left);
        Button rightButton = (Button)GetTemplateChild(PART_Right);

        _switch.Click += SwitchClick;
        _box.MouseLeftButtonUp += OnBoxLeftButtonUp;
        _popup.Closed += OnPopupClosed;
        leftButton.Click += (s, e) => MoveMonthClick(-1);
        rightButton.Click += (s, e) => MoveMonthClick(1);
    }

    private void MoveMonthClick(int month)
    {
        GenerateCalendar(CurrentMonth!.Value.AddMonths(month));
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        _switch!.IsChecked = IsMouseOver;
    }

    private void OnBoxLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_box!.SelectedItem is CalendarBoxItem selected)
        {
            SelectedDate = selected.Date;
            GenerateCalendar(selected.Date);
            _popup!.IsOpen = KeepPopupOpen.GetValueOrDefault();
        }
    }

    private void SwitchClick(object sender, RoutedEventArgs e)
    {
        if (_switch!.IsChecked.GetValueOrDefault())
        {
            GenerateCalendar(SelectedDate.GetValueOrDefault(DateTime.Now));
            _popup!.IsOpen = true;
        }
    }

    private void GenerateCalendar(DateTime current)
    {
        if (current.ToString("yyyyMM") == CurrentMonth.GetValueOrDefault().ToString("yyyyMM"))
        {
            return;
        }

        CurrentMonth = current;
        _box!.Items.Clear();
        DateTime fDayOfMonth = new(current.Year, current.Month, 1);
        DateTime lDayOfMonth = fDayOfMonth.AddMonths(1).AddDays(-1);

        int fOffset = (int)fDayOfMonth.DayOfWeek;
        int lOffset = 6 - (int)lDayOfMonth.DayOfWeek;

        DateTime fDay = fDayOfMonth.AddDays(-fOffset);
        DateTime lDay = lDayOfMonth.AddDays(lOffset);

        for (DateTime day = fDay; day <= lDay; day = day.AddDays(1))
        {
            var boxItem = new CalendarBoxItem() 
            { 
                Date = day,
                Content = day.Day,
                InCurrentMonth = day.Month == CurrentMonth.GetValueOrDefault().Month,
            };
            _box.Items.Add(boxItem);
            if (day == SelectedDate)
            {
                _box.SelectedItem = boxItem;
            }
        }
    }
}
