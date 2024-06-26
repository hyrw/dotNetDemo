using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ControlLibrary.UserControls;

/// <summary>
/// HPBar.xaml 的交互逻辑
/// </summary>
public partial class HPBar : UserControl
{

    public double HP
    {
        get { return (double)GetValue(HPProperty); }
        set { SetValue(HPProperty, value); }
    }
    public static readonly DependencyProperty HPProperty;

    readonly DoubleAnimation widthAnimation;

    public HPBar()
    {
        InitializeComponent();
        widthAnimation = new()
        {
            IsAdditive = true,
            IsCumulative = true,
            Duration = TimeSpan.FromSeconds(1),
            FillBehavior = FillBehavior.HoldEnd,
        };
    }

    static HPBar()
    {
        HPProperty = DependencyProperty.Register(nameof(HP), typeof(double), typeof(HPBar), new PropertyMetadata(default(double), OnHPChanged));
    }

    static void OnHPChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        HPBar hpBar = (HPBar)d;
        hpBar.widthAnimation.To = (double)e.NewValue;
        hpBar.hpRect.BeginAnimation(WidthProperty, hpBar.widthAnimation);
    }

}
