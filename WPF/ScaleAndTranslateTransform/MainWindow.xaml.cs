using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ScaleAndTranslateTransform;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const double ZoomFactor = 1.2;
    private const double MinZoom = 0.2;
    private const double MaxZoom = 5.0;

    private Point? lastMousePosition;
    private bool isDragging;
    Storyboard MatrixStoryboard => (Storyboard)FindResource("Storyboard.Matrix");
    Storyboard TranslateStoryboard => (Storyboard)FindResource("Storyboard.Translate");

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        this.MatrixStoryboard.Begin();
        this.matrixTransform.Matrix = Matrix.Identity;
    }

    private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        Point point = e.GetPosition(this.EleContainer);

        var matrix = this.transformGroup.Value;

        double delta = e.Delta > 0 ? ZoomFactor : 1 / ZoomFactor;

        // 方式1 使用MatrixTransform
        matrix.ScaleAtPrepend(delta, delta, point.X, point.Y);
        this.MatrixStoryboard.Begin();
        this.matrixTransform.Matrix = matrix;

        // 方式2 使用TransformGroup
        //this.scaleTransform.ScaleX = matrix.M11;
        //this.scaleTransform.ScaleY = matrix.M22;
        //this.translateTransform.X = matrix.OffsetX;
        //this.translateTransform.Y = matrix.OffsetY;

        // 与ScaleAtPrepend相等
        //matrix.Prepend(new Matrix
        //{
        //    OffsetX = point.X,
        //    OffsetY = point.Y,
        //});
        //matrix.Prepend(new Matrix
        //{
        //    M11 = delta,
        //    M22 = delta,
        //});
        //matrix.Prepend(new Matrix
        //{
        //    OffsetX = -point.X,
        //    OffsetY = -point.Y,
        //});

        // 与ScaleAtPrepend相等
        //matrix.Append(new Matrix
        //{
        //    OffsetX = -point.X,
        //    OffsetY = -point.Y,
        //});
        //matrix.Append(new Matrix
        //{
        //    M11 = delta,
        //    M22 = delta,
        //});
        //matrix.Append(new Matrix
        //{
        //    OffsetX = point.X,
        //    OffsetY = point.Y,
        //});
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDragging || !lastMousePosition.HasValue) return;

        e.Handled = true;

        Point mousePos = e.GetPosition(this.EleContainer);
        Vector delta = mousePos - lastMousePosition.Value;

        var matrix = this.transformGroup.Value;

        matrix.Translate(delta.X, delta.Y);

        this.TranslateStoryboard.Begin();
        this.matrixTransform.Matrix = matrix;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement ele) return;

        ele.CaptureMouse();
        lastMousePosition = e.GetPosition(this.EleContainer);
        isDragging = true;
        e.Handled = true;
    }

    private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement ele) return;

        ele.ReleaseMouseCapture();
        lastMousePosition = null;
        isDragging = false;
        e.Handled = true;
    }
}