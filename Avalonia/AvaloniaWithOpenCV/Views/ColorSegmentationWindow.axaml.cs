using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaWithOpenCV.Extensions;
using OpenCvSharp;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Window = Avalonia.Controls.Window;

namespace AvaloniaWithOpenCV;

public partial class ColorSegmentationWindow : Window
{

    public static readonly StyledProperty<Mat> ImgProperty;
    public Mat Img
    {
        get => GetValue(ImgProperty);
        set => SetValue(ImgProperty, value);
    }

    public static readonly StyledProperty<int> HMinProperty;
    public int HMin
    {
        get => GetValue(HMinProperty);
        set => SetValue(HMinProperty, value);
    }

    public static readonly StyledProperty<int> HMaxProperty;
    public int HMax
    {
        get => GetValue(HMaxProperty);
        set => SetValue(HMaxProperty, value);
    }

    public static readonly StyledProperty<int> SMinProperty;
    public int SMin
    {
        get => GetValue(SMinProperty);
        set => SetValue(SMinProperty, value);
    }

    public static readonly StyledProperty<int> SMaxProperty;
    public int SMax
    {
        get => GetValue(SMaxProperty);
        set => SetValue(SMaxProperty, value);
    }

    public static readonly StyledProperty<int> VMinProperty;
    public int VMin
    {
        get => GetValue(VMinProperty);
        set => SetValue(VMinProperty, value);
    }

    public static readonly StyledProperty<int> VMaxProperty;
    public int VMax
    {
        get => GetValue(VMaxProperty);
        set => SetValue(VMaxProperty, value);
    }

    public ColorSegmentationWindow()
    {
        InitializeComponent();
        Observable.FromEventPattern<AvaloniaPropertyChangedEventArgs>(h => this.PropertyChanged += h, h => this.PropertyChanged -= h)
            .Where(x =>
            {
                var property = x.EventArgs.Property;
                if (property == HMinProperty || property == HMaxProperty ||
                property == SMinProperty || property == SMaxProperty ||
                property == VMinProperty || property == VMaxProperty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            )
            .Throttle(TimeSpan.FromMicroseconds(100))
            .ObserveOn(AvaloniaSynchronizationContext.Current!)
            .Subscribe(async x => await this.UpdateImageAsync(this.Img));
    }

    static ColorSegmentationWindow()
    {
        ImgProperty = AvaloniaProperty.Register<ColorSegmentationWindow, Mat>(nameof(Img));
        HMinProperty = AvaloniaProperty.Register<ColorSegmentationWindow, int>(nameof(HMinProperty), 0);
        HMaxProperty = AvaloniaProperty.Register<ColorSegmentationWindow, int>(nameof(HMaxProperty), 255);
        SMinProperty = AvaloniaProperty.Register<ColorSegmentationWindow, int>(nameof(SMinProperty), 0);
        SMaxProperty = AvaloniaProperty.Register<ColorSegmentationWindow, int>(nameof(SMaxProperty), 255);
        VMinProperty = AvaloniaProperty.Register<ColorSegmentationWindow, int>(nameof(VMinProperty), 0);
        VMaxProperty = AvaloniaProperty.Register<ColorSegmentationWindow, int>(nameof(VMaxProperty), 255);
    }
    async void Drop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;
        var storeItem = e.Data.GetFiles()!;
        string localPath = storeItem.First().Path.LocalPath;

        Mat color = await ImReadAsync(localPath);
        this.Img = color;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ImgProperty)
        {
            (Mat oldValue, Mat newValue) = change.GetOldAndNewValue<Mat>();
            oldValue?.Dispose();
            _ = this.UpdateImageAsync(newValue);
        }
    }

    async Task UpdateImageAsync(Mat color)
    {
        if (color is null || color.Empty()) return;

        Scalar lower = new(this.HMin, this.SMin, this.VMin);
        Scalar upper = new(this.HMax, this.SMax, this.VMax);

        using Mat mask = await Task.Run(() => color.InRange(lower, upper));
        using Mat filteredColor = await Task.Run(() =>
        {
            Mat m = new();
            color.CopyTo(m, mask);
            return m;
        });

        Avalonia.Size size = new(color.Width, color.Height);
        Vector dpi = new(96, 96);

        WriteableBitmap? source = this.TheImage.Source as WriteableBitmap;
        await Task.Run(() =>
        {
            if (source is null || source.Size != size)
            {
                Avalonia.PixelSize pixelSize = new(filteredColor.Width, filteredColor.Height);
                source = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888);
                filteredColor.ToBitmapParallel(source);
            }
            else
            {
                filteredColor.ToBitmapParallel(source);
            }
        });
        this.TheImage.Source = source;
        this.TheImage.InvalidateVisual();
    }

    static Task<Mat> ImReadAsync(string file, ImreadModes flags = ImreadModes.Color) => Task.Run(() => Cv2.ImRead(file, flags));
}