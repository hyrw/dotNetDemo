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
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaWithOpenCV.Views;

public partial class ThresholdWindow : Avalonia.Controls.Window
{
    AxisLine? thresholdAxisLine = null;
    readonly Crosshair? crosshair = null;
    string file = string.Empty;
    int? threshold;

    public Mat? Img { get; set; }
    public Mat? Mask { get; set; }

    public ThresholdWindow()
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
            Mat mask = gray.Threshold(this.threshold.Value, 255, ThresholdTypes.Binary);
            this.Mask = mask;
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

    static Task<Mat> ImReadAsync(string file, ImreadModes flags = ImreadModes.Color)
    {
        return Task.Run(() => Cv2.ImRead(file, flags));
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
        this.Img = color;
        await UpdateImageAsync(this.Img);
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
        string localPath = storeItem.First().Path.LocalPath;

        this.file = localPath;
        Mat color = await ImReadAsync(localPath);
        this.Img = color;
        await UpdateImageAsync(color);
    }
}