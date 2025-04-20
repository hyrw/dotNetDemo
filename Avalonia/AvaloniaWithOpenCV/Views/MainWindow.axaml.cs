using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var thresholdWindow = new ThresholdWindow();
        await thresholdWindow.ShowDialog(this);
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
