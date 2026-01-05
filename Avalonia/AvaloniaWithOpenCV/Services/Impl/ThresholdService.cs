using Avalonia;
using AvaloniaWithOpenCV.Views;
using OpenCvSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Services.Impl;

public class ThresholdService : IThresholdService
{

    readonly MainWindow mainWindow;
    readonly ThresholdWindow thresholdWindow;
    readonly IImageDisplay display;
    public ThresholdService(MainWindow owner, ThresholdWindow thresholdWindow, IImageDisplay display)
    {
        this.mainWindow = owner;
        this.thresholdWindow = thresholdWindow;
        this.display = display;

        this.thresholdWindow.Closing += ThresholdWindow_Closing;
        this.thresholdWindow.PropertyChanged += Window_PropertyChanged;
    }

    readonly List<AvaloniaProperty> Propertys = [
        ThresholdWindow.ThresholdProperty,
        ThresholdWindow.MaxValueProperty,
    ];

    private async void Window_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not ThresholdWindow thresholdWindow) return;
        if (!Propertys.Contains(e.Property)) return;

        var src = mainWindow.Mat;
        if (src is null) return;
        if (src.Channels() != 0)
        {
            src = src.CvtColor(ColorConversionCodes.BGR2GRAY);
        }
        var threshold = thresholdWindow.Threshold;
        var maxValue = thresholdWindow.MaxValue;

        using Mat dst = new(src.Size(), src.Type());
        Cv2.Threshold(src, dst, threshold, maxValue, ThresholdTypes.Binary);

        await display.Display(dst);
    }

    private void ThresholdWindow_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        var window = (ThresholdWindow)sender!;
        window.Hide();
        e.Cancel = true;
    }

    public void Hide() => thresholdWindow.Hide();

    public void Show()
    {
        var src = mainWindow.Mat;
        if (src is null) return;

        thresholdWindow.UpdatePlot(src);
        thresholdWindow.Show();
    }

}
