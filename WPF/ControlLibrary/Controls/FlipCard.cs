using System.Windows;
using System.Windows.Controls;

namespace ControlLibrary.Controls;

public class FlipCard : Control
{
    public UIElement Front
    {
        get { return (UIElement)GetValue(FrontProperty); }
        set { SetValue(FrontProperty, value); }
    }
    public UIElement Back
    {
        get { return (UIElement)GetValue(BackProperty); }
        set { SetValue(BackProperty, value); }
    }

    public static readonly DependencyProperty FrontProperty;
    public static readonly DependencyProperty BackProperty;

    static FlipCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipCard), new FrameworkPropertyMetadata(typeof(FlipCard)));
        FrontProperty = DependencyProperty.Register(nameof(Front), typeof(UIElement), typeof(FlipCard), new PropertyMetadata(default));
        BackProperty = DependencyProperty.Register(nameof(Back), typeof(UIElement), typeof(FlipCard), new PropertyMetadata(default));
    }
}
