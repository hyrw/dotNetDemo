using InteractionDemo.Adorners;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace InteractionDemo;

internal static class Placeholder
{


    public static string GetText(DependencyObject obj)
    {
        return (string?)obj.GetValue(TextProperty);
    }

    public static void SetText(DependencyObject obj, string value)
    {
        obj.SetValue(TextProperty, value);
    }

    // Using a DependencyProperty as the backing store for Placeholder.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(nameof(TextProperty), 
                                            typeof(string), 
                                            typeof(Placeholder), 
                                            new UIPropertyMetadata(string.Empty, OnPlaceholderChangedCallback));

    private static void OnPlaceholderChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox) return;

        textBox.Loaded += TextBox_Loaded;
        textBox.TextChanged += TextBox_TextChanged;
        var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.VisibilityProperty, typeof(UIElement));
        dpd.AddValueChanged(textBox, OnVisibilityChanged);
    }

    private static void OnVisibilityChanged(object? sender, EventArgs e)
    {
        if (sender is not TextBox textBox) return;

        if (textBox.Visibility != Visibility.Visible)
        {

        }
    }

    private static void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;
        textBox.Loaded -= TextBox_Loaded;
        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (layer is null) return;
        if (string.IsNullOrEmpty(textBox.Text))
        {
            layer.Add(new PlaceholderAdorner(textBox, GetText(textBox)));
        }
    }

    private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        var layer = AdornerLayer.GetAdornerLayer(textBox);
        if (layer is null) return;

        var isShow = string.IsNullOrEmpty(textBox.Text);
        if (isShow)
        {
            layer.Add(new PlaceholderAdorner(textBox, GetText(textBox)));
        }
        else
        {
            var adorners = layer.GetAdorners(textBox);
            if (adorners is null) return;
            for (int i = 0; i < adorners.Length; i++)
            {
                if (adorners[i] is PlaceholderAdorner)
                {
                    layer.Remove(adorners[i]);
                }
            }
        }

    }
}
