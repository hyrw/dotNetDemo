using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ControlLibrary.Controls;

[TemplatePart(Name = PART_ITEMS, Type = typeof(ItemsControl))]
[TemplatePart(Name = PART_TRANSLATE, Type = typeof(TranslateTransform))]
public class NumberRoll : Control
{
    const string PART_ITEMS = "PART_items";
    const string PART_TRANSLATE = "PART_translate";
    public static readonly DependencyProperty ValueProperty;
    ItemsControl? itemsControl;
    TranslateTransform? translate;
    public int Value
    {
        get { return (int)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public NumberRoll()
    {
    }
    static NumberRoll()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberRoll), new FrameworkPropertyMetadata(typeof(NumberRoll)));

        ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumberRoll), new PropertyMetadata(0, OnValueChanged));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        this.itemsControl = (ItemsControl)Template.FindName(PART_ITEMS, this);
        this.translate = (TranslateTransform)Template.FindName(PART_TRANSLATE, this);
        if (this.itemsControl is null) return;

        this.itemsControl.Items.Clear();
        for (int i = 0; i < 10; i++)
        {
            this.itemsControl.Items.Add(i);
        }
    }

    private void RollAnimation(double y)
    {
        if (this.itemsControl is null) return;

        DoubleAnimation animation = new DoubleAnimation()
        {
            To = y,
            Duration = TimeSpan.FromSeconds(1),
            FillBehavior = FillBehavior.HoldEnd,
        };
        this.translate!.BeginAnimation(TranslateTransform.YProperty, animation);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NumberRoll roll = (NumberRoll)d;
        int oldValue = (int)e.OldValue;
        int newValue = (int)e.NewValue;
        if (newValue == oldValue) return;

        double translateY = -(newValue * roll.Height);
        roll.RollAnimation(translateY);
    }
}
