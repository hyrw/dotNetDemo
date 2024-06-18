using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows;

namespace WaterRipple;

// source http://note.youdao.com/s/ZsJLkpDu
[TemplatePart(Name = PART_Geometry, Type = typeof(EllipseGeometry))]
[TemplatePart(Name = PART_Path, Type = typeof(Path))]
public partial class WaterRippleButton : Button
{
    const string PART_Geometry = "PART_Geometry";
    const string PART_Path = "PART_Path";
    EllipseGeometry? _ellipseGeometry;
    Path? _path;

    readonly DoubleAnimation _radius;
    readonly DoubleAnimation _opacity;

    public WaterRippleButton()
    {
        InitializeComponent();

         _radius = new DoubleAnimation()
        {
            From = 0,
            To = 150,
            Duration = TimeSpan.FromSeconds(1)
        };
        _opacity = new DoubleAnimation()
        {
            From = 0.3,
            To = 0,
            Duration = TimeSpan.FromSeconds(1)
        };
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _ellipseGeometry = Template.FindName(PART_Geometry, this) as EllipseGeometry;
        _path = Template.FindName(PART_Path, this) as Path;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        _ellipseGeometry!.Center = Mouse.GetPosition(this);
        _ellipseGeometry.BeginAnimation(EllipseGeometry.RadiusXProperty, _radius);
        _path!.BeginAnimation(Path.OpacityProperty, _opacity);

        e.Handled = true;
    }

}