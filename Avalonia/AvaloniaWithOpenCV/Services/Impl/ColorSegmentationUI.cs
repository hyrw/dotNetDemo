using Avalonia;
using AvaloniaWithOpenCV.Views;
using OpenCvSharp;
using System.Collections.Generic;

namespace AvaloniaWithOpenCV.Services.Impl;

internal class ColorSegmentationUI : IColorSegmentationUI
{
    readonly MainWindow mainWindow;
    readonly ColorSegmentationWindow window;
    readonly IImageDisplay display;

    public ColorSegmentationUI(MainWindow mainWindow, ColorSegmentationWindow window, IImageDisplay display)
    {
        this.mainWindow = mainWindow;
        this.window = window;
        this.display = display;

        this.window.PropertyChanged += Window_PropertyChanged;
        this.window.Closing += Window_Closing;
    }

    private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        ColorSegmentationWindow window = (ColorSegmentationWindow)sender!;
        window.Hide();
        e.Cancel = true;
    }

    public void Show() => window.Show();

    readonly List<AvaloniaProperty> ChannelPropertys = [
        ColorSegmentationWindow.HMinProperty,
        ColorSegmentationWindow.HMaxProperty,
        ColorSegmentationWindow.SMinProperty,
        ColorSegmentationWindow.SMaxProperty,
        ColorSegmentationWindow.VMinProperty,
        ColorSegmentationWindow.VMaxProperty
    ];

    private async void Window_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!ChannelPropertys.Contains(e.Property)) return;

        var window = (ColorSegmentationWindow)sender!;

        Scalar lower = new(window.HMin, window.SMin, window.VMin);
        Scalar upper = new(window.HMax, window.SMax, window.VMax);

        // TODO: 使用别的方式获取图片
        Mat? src = mainWindow.Mat;
        if (src is null) return;

        using Mat mask = src.InRange(lower, upper);

        using Mat dst = new(src.Size(), src.Type());
        Cv2.CopyTo(src, dst, mask);

        await display.Display(dst);
    }

    public void Hide() => window.Hide();
}
