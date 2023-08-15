using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WaterRipple;

// source http://note.youdao.com/s/ZsJLkpDu
public partial class WaterRippleButton : Button
{
    public WaterRippleButton()
    {
        InitializeComponent();
    }

    private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var expansionAnimation = new DoubleAnimation()
        {
            From = 0,
            To = 150,
            Duration = TimeSpan.FromSeconds(1)
        };
        var opacityAnimation = new DoubleAnimation()
        {
            From = 0.3,
            To = 0,
            Duration = TimeSpan.FromSeconds(1)
        };
        
        var ellipseGeometry = Template.FindName("_geometry", this) as EllipseGeometry;
        var path = Template.FindName("_path", this) as Path;
        
        ellipseGeometry!.Center = Mouse.GetPosition(this);
        ellipseGeometry.BeginAnimation(EllipseGeometry.RadiusXProperty, expansionAnimation);
        
        path!.BeginAnimation(Path.OpacityProperty, opacityAnimation);
    }
}