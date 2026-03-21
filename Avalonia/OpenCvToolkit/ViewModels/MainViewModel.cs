using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using OpenCvToolkit.Extensions;
using OpenCvToolkit.Messages;
using System.IO;
using CvPoint = OpenCvSharp.Point;

namespace OpenCvToolkit.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial WriteableBitmap? Before { get; set; }

    [ObservableProperty]
    public partial WriteableBitmap? After { get; set; }

    [ObservableProperty]
    public partial Matrix SyncMatrix { get; set; } = Matrix.Identity;

    [ObservableProperty]
    public partial string FilePath { get; set; } = @"D:\Pictures\Camera Roll\cat.jpeg";

    partial void OnFilePathChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !File.Exists(value)) return;

        LoadImageCommand.Execute(null);
    }

    public MainViewModel()
    {
    }

    [RelayCommand]
    void LoadImage()
    {
        if (string.IsNullOrWhiteSpace(FilePath) || !File.Exists(FilePath)) return;

        using Mat src = Cv2.ImRead(FilePath, ImreadModes.Grayscale);
        using Mat result = src.Threshold(220, 255, ThresholdTypes.Binary).Clone();

        FloodFill(result);

        Before = Update(src, Before);
        After = Update(result, After);

        WeakReferenceMessenger.Default.Send(new UpdateImageMessage());
    }

    static void FloodFill(Mat input)
    {
        Cv2.FloodFill(input, new CvPoint(0, 0), Scalar.Black);
    }

    static WriteableBitmap Update(Mat img, WriteableBitmap? writeableBitmap)
    {
        Avalonia.Size size = new(img.Width, img.Height);
        Vector dpi = new(96, 96);

        if (writeableBitmap is null || writeableBitmap.Size != size)
        {
            PixelSize pixelSize = new(img.Width, img.Height);
            writeableBitmap = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888);
            img.ToBitmapParallel(writeableBitmap);
        }
        else
        {
            img.ToBitmapParallel(writeableBitmap);
        }
        return writeableBitmap;
    }

}
