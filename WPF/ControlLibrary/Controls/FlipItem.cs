using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ControlLibrary.Controls;

[TemplatePart(Name = PART_Before_Top, Type = typeof(Viewport2DVisual3D))]
[TemplatePart(Name = PART_After_Bottom, Type = typeof(Viewport2DVisual3D))]
[TemplatePart(Name = PART_Before_Bottom, Type = typeof(Viewport2DVisual3D))]
public class FlipItem : Control
{
    const string PART_Before_Top = "PART_before_top";
    const string PART_After_Bottom = "PART_after_bottom";
    const string PART_Before_Bottom = "PART_before_bottom";

    public static readonly DependencyProperty BeforeValueProperty;
    public static readonly DependencyProperty AfterValueProperty;

    Viewport2DVisual3D? before_top;
    Viewport2DVisual3D? after_bottom;
    Viewport2DVisual3D? before_bottom;
    readonly Storyboard? storyboard;
    readonly DoubleAnimation? doubleAnimation1;
    readonly DoubleAnimation? doubleAnimation2;
    readonly DoubleAnimation? doubleAnimation3;

    private string BeforeValue
    {
        get { return (string)GetValue(BeforeValueProperty); }
        set { SetValue(BeforeValueProperty, value); }
    }

    private string AfterValue
    {
        get { return (string)GetValue(AfterValueProperty); }
        set { SetValue(AfterValueProperty, value); }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        before_top = (Viewport2DVisual3D)this.Template.FindName(PART_Before_Top, this);
        after_bottom = (Viewport2DVisual3D)this.Template.FindName(PART_After_Bottom, this);
        before_bottom = (Viewport2DVisual3D)this.Template.FindName(PART_Before_Bottom, this);

        Storyboard.SetTarget(doubleAnimation1, before_top);
        Storyboard.SetTargetProperty(doubleAnimation1, new PropertyPath("(Visual3D.Transform).(RotateTransform3D.Rotation).(AxisAngleRotation3D.Angle)"));

        Storyboard.SetTarget(doubleAnimation2, after_bottom);
        Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(Visual3D.Transform).(RotateTransform3D.Rotation).(AxisAngleRotation3D.Angle)"));

        Storyboard.SetTarget(doubleAnimation3, before_bottom);
        Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("(Visual3D.Transform).(RotateTransform3D.Rotation).(AxisAngleRotation3D.Angle)"));
    }

    private void Storyboard_Completed(object? sender, EventArgs e)
    {
        BeforeValue = AfterValue;
    }

    public void StarAnimation()
    {
        if (storyboard is null) return;

        storyboard.Begin(this, true);
    }

    public FlipItem()
    {
        storyboard = new Storyboard();
        storyboard.Completed += Storyboard_Completed;
        doubleAnimation1 = new DoubleAnimation()
        {
            BeginTime = TimeSpan.Zero,
            From = 0,
            To = 90,
            Duration = TimeSpan.FromSeconds(0.5),
        };
        
        doubleAnimation2 = new DoubleAnimation()
        {
            From = -90,
            BeginTime = TimeSpan.FromSeconds(0.5),
            To = 0,
            Duration = TimeSpan.FromSeconds(0.5),
        };

        doubleAnimation3 = new DoubleAnimation()
        {
            BeginTime = TimeSpan.FromSeconds(1),
            From = 0,
            To = 90,
        };
        storyboard.Children.Add(doubleAnimation1);
        storyboard.Children.Add(doubleAnimation2);
        storyboard.Children.Add(doubleAnimation3);
    }

    static FlipItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipItem), new FrameworkPropertyMetadata(typeof(FlipItem)));

        BeforeValueProperty = DependencyProperty.Register(nameof(BeforeValue), typeof(string), typeof(FlipItem), new PropertyMetadata("0"));
        AfterValueProperty = DependencyProperty.Register(nameof(AfterValue), typeof(string), typeof(FlipItem), new PropertyMetadata("1"));
    }
}
