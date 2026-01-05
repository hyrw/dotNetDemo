using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaWithOpenCV.Extensions;
using OpenCvSharp;

namespace AvaloniaWithOpenCV.Services.Impl;

public class ImageDisplay(Image imgControl) : IImageDisplay
{

    public async Task Display(Mat mat)
    {
        Avalonia.Size size = new(mat.Width, mat.Height);
        Vector dpi = new(96, 96);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (imgControl.Source is not WriteableBitmap source || source.Size != size)
            {
                source = new WriteableBitmap(new PixelSize(mat.Width, mat.Height), dpi, PixelFormat.Bgra8888);
                mat.ToBitmapParallel(source);
            }
            else
            {
                mat.ToBitmapParallel(source);
            }
            imgControl.Source = source;
            imgControl.InvalidateVisual();
        });
    }
}
