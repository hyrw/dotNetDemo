using Avalonia;
using Avalonia.Controls;
using System;

namespace AvaloniaApplication.Controls;

public partial class FlashTextBlock : UserControl
{
    public static readonly StyledProperty<string?> TextProperty;
    public String? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<bool> EnableFlashProperty;
    public bool EnableFlash
    {
        get => GetValue(EnableFlashProperty);
        set => SetValue(EnableFlashProperty, value);
    }

    public FlashTextBlock()
    {
        InitializeComponent();
    }

    static FlashTextBlock()
    {
        TextProperty = AvaloniaProperty.Register<FlashTextBlock, string?>(nameof(Text));
        EnableFlashProperty = AvaloniaProperty.Register<FlashTextBlock, bool>(nameof(EnableFlash), defaultValue: true);
    }
}