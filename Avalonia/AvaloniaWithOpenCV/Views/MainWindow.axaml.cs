using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Transformation;
using AvaloniaWithOpenCV.ViewModels;
using OpenCvSharp;
using System.IO;

namespace AvaloniaWithOpenCV.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (this.EleContainer.RenderTransform is null) return;
        e.Handled = true;

        var p = e.GetCurrentPoint(this.EleContainer);
        double scale = e.Delta.Y > 0 ? 1.2 : 1 / 1.2;

        var m = this.EleContainer.RenderTransform.Value;
        m = m.ScaleAtPrepend(scale, scale, p.Position.X, p.Position.Y);

        // 不用动画可以直接用MatrixTransform
        var t = TransformOperations.Parse($"matrix({m.M11},{m.M21},{m.M21},{m.M22},{m.M31},{m.M32})");
        this.EleContainer.RenderTransform = t;
    }

    public Mat? Mat { get; set; }

    async void Drop(object? sender, DragEventArgs e)
    {
        var storeItem = e.DataTransfer.TryGetFile();
        string localPath = storeItem?.Path.LocalPath ?? string.Empty;
        if (!File.Exists(localPath)) return;

        if (this.DataContext is not MainWindowViewModel vm) return;

        vm.FilePath = localPath;
        Mat?.Dispose();
        Mat = Cv2.ImRead(localPath);
    }
}

public static class MatrixEx
{
    public static Matrix ScaleAtPrepend(this Matrix m, double scaleX, double scaleY, double x, double y)
    {
        m = Matrix.CreateTranslation(x, y) * m;
        m = Matrix.CreateScale(scaleX, scaleY) * m;
        m = Matrix.CreateTranslation(-x, -y) * m;
        return m;
    }
}
