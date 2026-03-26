using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenCvSharp;
using OpenCvToolkit.Extensions;
using OpenCvToolkit.Messages;
using System;
using System.IO;
using System.Linq;
using CvPoint = OpenCvSharp.Point;

namespace OpenCvToolkit.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    static readonly string[] ImageExtensionNames = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp"];

    [ObservableProperty]
    public partial WriteableBitmap? Before { get; set; }

    [ObservableProperty]
    public partial WriteableBitmap? After { get; set; }

    [ObservableProperty]
    public partial Matrix SyncMatrix { get; set; } = Matrix.Identity;

    [ObservableProperty]
    public partial string FilePath { get; set; } = string.Empty;

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


    [RelayCommand]
    void DropImage(DragEventArgs e)
    {
        if (!TryGetImageFile(e, out string file)) return;

        FilePath = file;
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

    static bool TryGetImageFile(DragEventArgs e, out string file)
    {
        file = string.Empty;
        if (!e.Data.Contains(DataFormats.Files)) return false;

        var storeItems = e.Data.GetFiles()?.ToList();
        if (storeItems is null || storeItems.Count != 1) return false;

        var filePath = storeItems[0].Path.LocalPath;
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        if ( !string.IsNullOrWhiteSpace(extension) && ImageExtensionNames.Contains(extension))
        {
            file = filePath;
            return true;
        }
        else
        {
            return false;
        }
    }

}
