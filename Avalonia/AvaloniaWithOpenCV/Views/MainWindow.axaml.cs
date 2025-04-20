using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OpenCvSharp;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot.Statistics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    Queue<string> fileQueue = new();

    AxisLine? thresholdAxisLine = null;
    readonly Crosshair? crosshair = null;
    string file = string.Empty;
    int? threshold;

    public MainWindow()
    {
        InitializeComponent();
        this.crosshair = this.AvaPlot.Plot.Add.Crosshair(0, 0);
        this.crosshair.TextColor = Colors.White;
        this.crosshair.TextBackgroundColor = crosshair.HorizontalLine.Color;
        this.crosshair.HorizontalLine.IsVisible = false;
        this.crosshair.VerticalLine.IsVisible = false;
        var vl = this.AvaPlot.Plot.Add.VerticalLine(0);
        vl.IsDraggable = true;
        vl.Text = "Threshold";

        // threshold axis
        this.AvaPlot.PointerPressed += OnMouseDown;
        this.AvaPlot.PointerReleased += OnMouseUp;
        this.AvaPlot.PointerMoved += OnMouseMove;

        // crosshair
        this.AvaPlot.PointerMoved += CrosshairHandle;

        // Hide axis label and tick
        this.AvaPlot.Plot.Axes.Left.IsVisible = false;
        // this.AvaPlot.Plot.Axes.Left.TickLabelStyle.IsVisible = false;
        // this.AvaPlot.Plot.Axes.Left.MinorTickStyle.Length = 0;
        this.AvaPlot.Plot.Axes.Right.IsVisible = false;
        // this.AvaPlot.Plot.Axes.Right.TickLabelStyle.IsVisible = false;
        // this.AvaPlot.Plot.Axes.Right.MinorTickStyle.Length = 0;

        // Hide axis edge line
        this.AvaPlot.Plot.Axes.Top.FrameLineStyle.Width = 0;
        this.AvaPlot.Plot.Axes.Left.FrameLineStyle.Width = 0;
        this.AvaPlot.Plot.Axes.Right.FrameLineStyle.Width = 0;
    }

    async Task UpdateImageAsync(Mat color)
    {
        Mat gray = await Task.Run(() => color.CvtColor(ColorConversionCodes.BGR2GRAY));
        Mat grayToColor = await Task.Run(() => gray.CvtColor(ColorConversionCodes.GRAY2BGR));

        if (this.threshold.HasValue)
        {
            using Mat mask = gray.Threshold(this.threshold.Value, 255, ThresholdTypes.Binary);
            grayToColor.SetTo(Scalar.Red, mask);
        }

        Avalonia.Size size = new(gray.Width, gray.Height);
        Vector dpi = new(96, 96);

        WriteableBitmap? source = this.TheImage.Source as WriteableBitmap;
        await Task.Run(() =>
        {
            if (source is null || source.Size != size)
            {
                Avalonia.PixelSize pixelSize = new(grayToColor.Width, grayToColor.Height);
                source = new WriteableBitmap(pixelSize, dpi, PixelFormat.Bgra8888);
                grayToColor.ToBitmapParallel(source);
            }
            else
            {
                grayToColor.ToBitmapParallel(source);
            }
        });
        this.TheImage.Source = source;

        (int width, int height) = (gray.Size());
        var pool = ArrayPool<double>.Shared;
        double[] values = pool.Rent(width * height);
        try
        {
            await Parallel.ForAsync(0, height, (y, cancelToken) =>
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    byte v = gray.At<byte>(y, x);
                    values[index] = v;
                }
                return ValueTask.CompletedTask;
            });
            await Task.Run(() =>
            {
                var histogram = Histogram.WithBinCount(256, 0, 255);
                histogram.AddRange(values);
                this.AvaPlot.Plot.Clear<HistogramBars>();
                this.AvaPlot.Plot.Add.Histogram(histogram);
            });
        }
        finally
        {
            pool.Return(values);
            gray.Dispose();
            grayToColor.Dispose();
        }
        this.AvaPlot.Refresh();
        this.TheImage.InvalidateVisual();
    }

    Task<Mat> ImReadAsync(string file, ImreadModes imreadModes = ImreadModes.Color)
    {
        return Task.Run(() => Cv2.ImRead(file, imreadModes));
    }

    private void OnMouseDown(object? sender, PointerEventArgs e)
    {
        if (sender is not AvaPlot avaPlot) return;

        var pos = e.GetPosition(avaPlot);
        var lineUnderMouse = GetLineUnderMouse((float)pos.X, (float)pos.Y, avaPlot);
        if (lineUnderMouse is not null)
        {
            thresholdAxisLine = lineUnderMouse;
            avaPlot.UserInputProcessor.Disable(); // disable panning while dragging
        }
    }

    private async void OnMouseUp(object? sender, PointerEventArgs e)
    {
        if (sender is not AvaPlot avaPlot) return;

        thresholdAxisLine = null;
        avaPlot.UserInputProcessor.Enable(); // enable panning again
        Mat color = await ImReadAsync(this.file);
        await UpdateImageAsync(color);
        avaPlot.Refresh();
    }

    void CrosshairHandle(object? sender, PointerEventArgs e)
    {
        if (sender is not AvaPlot avaPlot || this.crosshair is null) return;

        var pos = e.GetPosition(avaPlot);

        Pixel mousePixel = new(pos.X, pos.Y);
        Coordinates mouseCoordinates = avaPlot.Plot.GetCoordinates(mousePixel);
        Title = $"X={mouseCoordinates.X:N1}, Y={mouseCoordinates.Y:N1}";

        crosshair.Position = mouseCoordinates;
        crosshair.VerticalLine.Text = $"{mouseCoordinates.X:N1}";
        // crosshair.HorizontalLine.Text = $"{mouseCoordinates.Y:N1}";
        avaPlot.Refresh();
    }

    private void OnMouseMove(object? sender, PointerEventArgs e)
    {
        if (sender is not AvaPlot avaPlot) return;

        var pos = e.GetPosition(avaPlot);

        // this rectangle is the area around the mouse in coordinate units
        CoordinateRect rect = avaPlot.Plot.GetCoordinateRect((float)pos.X, (float)pos.Y, radius: 10);
        if (rect.HorizontalCenter < 0 ||
            rect.HorizontalCenter > 255) return;

        if (thresholdAxisLine is null)
        {
            // set cursor based on what's beneath the plottable
            var lineUnderMouse = GetLineUnderMouse((float)pos.X, (float)pos.Y, avaPlot);
            if (lineUnderMouse is null) Cursor = new(StandardCursorType.Arrow);
            else if (lineUnderMouse.IsDraggable && lineUnderMouse is VerticalLine) Cursor = new(StandardCursorType.SizeWestEast);
            else if (lineUnderMouse.IsDraggable && lineUnderMouse is HorizontalLine) Cursor = new(StandardCursorType.SizeNorthSouth);
        }
        else
        {
            // update the position of the plottable being dragged
            if (thresholdAxisLine is HorizontalLine hl)
            {
                hl.Y = rect.VerticalCenter;
                hl.Text = $"{hl.Y:0.00}";
            }
            else if (thresholdAxisLine is VerticalLine vl)
            {
                vl.X = rect.HorizontalCenter;
                vl.Text = $"{vl.X:0.00}";
                this.threshold = (int)vl.X;
            }
            avaPlot.Refresh();
        }
    }

    private AxisLine? GetLineUnderMouse(float x, float y, AvaPlot avaPlot)
    {
        CoordinateRect rect = avaPlot.Plot.GetCoordinateRect(x, y, radius: 10);

        foreach (AxisLine axLine in avaPlot.Plot.GetPlottables<AxisLine>().Reverse())
        {
            if (axLine.IsUnderMouse(rect))
                return axLine;
        }

        return null;
    }

    async void Drop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.Files)) return;
        var storeItem = e.Data.GetFiles()!;
        List<string> filePath = [];
        foreach (var i in storeItem)
        {
            string path = i.Path.LocalPath;
            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
                filePath.AddRange(files);
            }
            else if (File.Exists(path))
            {
                filePath.Add(path);
            }
        }
        foreach (var i in filePath)
        {
            this.fileQueue.Enqueue(i);
        }

        if (this.fileQueue.TryDequeue(out string? file) && !string.IsNullOrEmpty(file))
        {
            this.file = file;
            using Mat color = await ImReadAsync(this.file);
            await UpdateImageAsync(color);
        }
    }
}

// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information. 
public static class PixelFormatExtensions
{
    public static int GetBytesPerPixel(this PixelFormat pixelFormat)
    {
        if (PixelFormat.Rgb565.Equals(pixelFormat)) return 2;
        if (PixelFormat.Rgba8888.Equals(pixelFormat)) return 4;
        if (PixelFormat.Bgra8888.Equals(pixelFormat)) return 4;

        throw new ArgumentOutOfRangeException(nameof(pixelFormat), pixelFormat, null);
    }
}

public static class LockedFramebufferExtensions
{
    public static Span<byte> GetPixels(this ILockedFramebuffer framebuffer)
    {
        unsafe
        {
            return new Span<byte>((byte*)framebuffer.Address.ToPointer(), framebuffer.RowBytes * framebuffer.Size.Height);
        }
    }

    public static Span<byte> GetPixel(this ILockedFramebuffer framebuffer, int x, int y)
    {
        unsafe
        {
            var bytesPerPixel = framebuffer.Format.GetBytesPerPixel();
            var zero = (byte*)framebuffer.Address;
            var offset = framebuffer.RowBytes * y + bytesPerPixel * x;
            return new Span<byte>(zero + offset, bytesPerPixel);
        }
    }

    public static void SetPixel(this ILockedFramebuffer framebuffer, int x, int y, Avalonia.Media.Color color)
    {
        var pixel = framebuffer.GetPixel(x, y);

        var alpha = color.A / 255.0;

        var frameBufferFormat = framebuffer.Format;

        if (PixelFormat.Rgb565.Equals(frameBufferFormat))
        {
            var value = (((color.R & 0b11111000) << 8) + ((color.G & 0b11111100) << 3) + (color.B >> 3));
            pixel[0] = (byte)value;
            pixel[1] = (byte)(value >> 8);
        }
        else if (PixelFormat.Rgba8888.Equals(frameBufferFormat))
        {
            pixel[0] = (byte)(color.R * alpha);
            pixel[1] = (byte)(color.G * alpha);
            pixel[2] = (byte)(color.B * alpha);
            pixel[3] = color.A;
        }
        else if (PixelFormat.Bgra8888.Equals(frameBufferFormat))
        {
            pixel[0] = (byte)(color.B * alpha);
            pixel[1] = (byte)(color.G * alpha);
            pixel[2] = (byte)(color.R * alpha);
            pixel[3] = color.A;
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }

    }

    /// <summary>
    ///     Converts Mat to WriteableBitmap.
    ///     This method is more efficient because new instance of WriteableBitmap is not allocated.
    ///     Original:
    ///     https://github.com/shimat/opencvsharp/blob/master/src/OpenCvSharp.WpfExtensions/WriteableBitmapConverter.cs
    /// </summary>
    /// <param name="mat">Input Mat</param>
    /// <param name="dst">Output WriteableBitmap</param>
    //[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static void ToBitmapParallel(this Mat mat, WriteableBitmap dst)
    {
        ArgumentNullException.ThrowIfNull(mat);
        ArgumentNullException.ThrowIfNull(dst);

        if (mat.Width != dst.PixelSize.Width || mat.Height != dst.PixelSize.Height)
            throw new ArgumentException("size of src must be equal to size of dst");
        if (dst.Format != PixelFormat.Bgra8888 || !dst.Dpi.Equals(new Vector(96, 96)))
            throw new ArgumentException("Currently only Bgra8888 + 96 DPI WriteableBitmaps can be reused ");


        var width = mat.Width;
        var height = mat.Height;

        using var lockBuffer = dst.Lock();
        var stride = lockBuffer.RowBytes;
        var bufferAddress = lockBuffer.Address;
        unsafe
        {
            switch (mat.Channels())
            {
                // Method 1 (Span copy)
                case 1:
                    Parallel.For(0, height, y =>
                    {
                        var spanBitmap = new Span<uint>(IntPtr.Add(bufferAddress, y * stride).ToPointer(), width); // lockBuffer.GetPixelRowSpan(y,width);
                        var spanMat = mat.GetRowSpan<byte>(y, width);
                        for (var x = 0; x < width; x++)
                        {
                            var color = spanMat[x];
                            spanBitmap[x] = (uint)(color | (color << 8) | (color << 16) | (0xff << 24));
                        }
                    });
                    break;
                case 3:
                    Parallel.For(0, height, y =>
                    {
                        var spanBitmap = new Span<uint>(IntPtr.Add(bufferAddress, y * stride).ToPointer(), width); // lockBuffer.GetPixelRowSpan(y,width);
                        var spanMat = mat.GetRowSpan<byte>(y);
                        var pixel = 0;
                        for (var x = 0; x < width; x++) spanBitmap[x] = (uint)(spanMat[pixel++] | (spanMat[pixel++] << 8) | (spanMat[pixel++] << 16) | (0xff << 24));
                    });

                    break;
                case 4:
                    if (mat.Type() == MatType.CV_8U)
                    {
                        Parallel.For(0, height, y =>
                        {
                            var spanBitmap = new Span<uint>(IntPtr.Add(bufferAddress, y * stride).ToPointer(), width); // lockBuffer.GetPixelRowSpan(y,width);
                            var spanMat = mat.GetRowSpan<byte>(y);
                            var pixel = 0;
                            for (var x = 0; x < width; x++) spanBitmap[x] = (uint)(spanMat[pixel++] | (spanMat[pixel++] << 8) | (spanMat[pixel++] << 16) | (spanMat[pixel++] << 24));
                        });
                    }
                    //else if (mat.Type() == MatType.CV_32S)
                    //{
                    //    var dataCount = width * height;
                    //    mat.GetDataSpan<uint>().CopyTo(new Span<uint>(lockBuffer.Address.ToPointer(), dataCount));
                    //}

                    break;
            }
        }
    }

    public static unsafe Span<T> GetRowSpan<T>(this Mat mat, int y, int length = 0, int offset = 0)
    {
        return new(IntPtr.Add(mat.DataStart, y * mat.GetRealStep() + offset).ToPointer(), length <= 0 ? mat.GetRealStep() : length);
    }

    public static int GetRealStep(this Mat mat) => mat.Width * mat.Channels();
}
