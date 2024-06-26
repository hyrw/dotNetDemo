using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ControlLibrary.UserControls;

/// <summary>
/// HPBar.xaml 的交互逻辑
/// </summary>
public partial class HPBar : UserControl
{

    public static readonly DependencyProperty HPProperty;
    public static readonly DependencyProperty MaxHPProperty;

    public double HP
    {
        get => (double)GetValue(HPProperty);
        set => SetValue(HPProperty, value);
    }

    public double MaxHP
    {
        get => (double)GetValue(MaxHPProperty);
        set => SetValue(MaxHPProperty, value);
    }

    readonly DoubleAnimation widthAnimation;

    public HPBar()
    {
        InitializeComponent();
        widthAnimation = new()
        {
            IsAdditive = true,
            IsCumulative = true,
            FillBehavior = FillBehavior.HoldEnd,
        };
    }

    static HPBar()
    {
        HPProperty = DependencyProperty.Register(nameof(HP), typeof(double), typeof(HPBar), new PropertyMetadata(default(double), OnHPChanged));
        MaxHPProperty = DependencyProperty.Register(nameof(MaxHP), typeof(double), typeof(HPBar), new PropertyMetadata(default(double), OnMaxHPChanged));
    }

    private static void OnMaxHPChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        HPBar hpBar = (HPBar)d;
        hpBar.widthAnimation.To = hpBar.HP2Width(hpBar.HP);
        hpBar.widthAnimation.Duration = TimeSpan.Zero;
        hpBar.hpRect.BeginAnimation(WidthProperty, hpBar.widthAnimation);
    }

    static void OnHPChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        HPBar hpBar = (HPBar)d;
        if (hpBar.MaxHP <= 0) return;

        hpBar.widthAnimation.To = hpBar.HP2Width((double)e.NewValue);
        hpBar.widthAnimation.Duration = TimeSpan.FromSeconds(1);
        hpBar.hpRect.BeginAnimation(WidthProperty, hpBar.widthAnimation);
    }

    double HP2Width(double hp)
    {
        return hp / MaxHP * Width;
    }
}
