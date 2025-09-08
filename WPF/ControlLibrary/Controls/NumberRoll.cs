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

    readonly Storyboard storyboard;
    readonly DoubleAnimation animation;
    bool animationPlaying;
    int lastNum = 0;
    public int Value
    {
        get { return (int)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public NumberRoll()
    {
        animation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(1),
            FillBehavior = FillBehavior.Stop,
        };
        Storyboard.SetTarget(animation, this.itemsControl);
        Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
        storyboard = new Storyboard {};
        storyboard.Children.Add(animation);
        storyboard.Completed += StoryboardCompleted;
    }
    static NumberRoll()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberRoll), new FrameworkPropertyMetadata(typeof(NumberRoll)));

        ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumberRoll), new PropertyMetadata(0, OnValueChanged));
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        NumberRoll roll = (NumberRoll)d;
        if (roll.animationPlaying) return;

        roll.RollAnimation();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        this.itemsControl = (ItemsControl)Template.FindName(PART_ITEMS, this);
        this.translate = (TranslateTransform)Template.FindName(PART_TRANSLATE, this);

        FillNumbers(0);
    }

    private void RollAnimation()
    {
        if (lastNum == Value) return;

        animation.To = GetOffsetY(lastNum, Value, Height);
        storyboard.Begin(this.itemsControl, true);
        animationPlaying = true;
        lastNum = Value;
    }

    private void StoryboardCompleted(object? sender, EventArgs e)
    {
        animationPlaying = false;
        FillNumbers(lastNum);
        this.translate!.Y = 0;
        if (Value != lastNum)
        {
            this.RollAnimation();
        }
    }

    private static double GetOffsetY(int oldValue, int newValue, double height)
    {
        double y;
        if (newValue > oldValue)
        {
            y = -(newValue - oldValue) * height;
        }
        else if (newValue == oldValue)
        {
            y = 0;
        }
        else
        {
            y = -(10 - oldValue + newValue) * height;
        }
        return y;
    }

    /// <summary>
    /// 从指定值开始填充10个数，
    /// </summary>
    /// <param name="value">0 - 9</param>
    private void FillNumbers(int value)
    {
        if (value > 9 || value < 0) throw new ArgumentException($"{nameof(value)} 必须在 0 - 9 的之间");

        this.itemsControl!.Items.Clear();
        for (int i = 0; i < 10; i++)
        {
            this.itemsControl.Items.Add(value);
            if (value == 9)
            {
                value = 0;
            }
            else
            {
                value++;
            }
        }
    }
}
