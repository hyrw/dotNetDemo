using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InteractionDemo.Behaviors;

/// <summary>
/// 占位符
/// </summary>
public class TextBoxPlaceholderBehavior : Behavior<UIElement>
{
    private Brush? foreground;//水印颜色
    public Brush? IsNotNullForeground { get; set; }

    public string? WaterMarkText { get; set; }

    protected override void OnAttached()
    {
        if (base.AssociatedObject is not TextBox textBox) return;

        foreground = textBox.Foreground;
        textBox.LostFocus += OnLostFocus;
        textBox.GotFocus += OnGotFocus;

        if (string.IsNullOrEmpty(textBox.Text))
        {
            textBox.Foreground = IsNotNullForeground;
            textBox.Text = WaterMarkText;
        }
    }

    protected override void OnDetaching()
    {
        if (base.AssociatedObject is not TextBox textBox) return;

        textBox.LostFocus -= OnLostFocus;
        textBox.GotFocus -= OnGotFocus;
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        if (string.IsNullOrEmpty(textBox.Text) || textBox.Text == WaterMarkText)
        {
            textBox.Foreground = IsNotNullForeground;
            textBox.Text = WaterMarkText;
        }
        else
        {
            textBox.Foreground = foreground;
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox) sender;
        if (string.IsNullOrEmpty(textBox.Text) || textBox.Text == WaterMarkText)
        {
            textBox.Foreground = IsNotNullForeground;
            textBox.Text = WaterMarkText;
        }
        else
        {
            textBox.Foreground = foreground;
        }
    }

}
