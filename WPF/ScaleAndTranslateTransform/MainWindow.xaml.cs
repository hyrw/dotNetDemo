using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.EleContainer.RenderTransform is MatrixTransform transform)
        {
            transform.Matrix = Matrix.Identity;
            e.Handled = true;
        }
    }

    private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not Panel || this.EleContainer.RenderTransform is not MatrixTransform transform) return;

        e.Handled = true;

        Point point = e.GetPosition(this.EleContainer);

        var matrix = transform.Value;

        double delta = e.Delta > 0 ? ZoomFactor : 1 / ZoomFactor;

        matrix.ScaleAtPrepend(delta, delta, point.X, point.Y);
        //Trace.WriteLine($"Scale x:{matrix.M11}, y:{matrix.M22}");

        transform.Matrix = matrix;
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Panel ||
            this.EleContainer.RenderTransform is not MatrixTransform matrixTransform) return;
        if (!isDragging || !lastMousePosition.HasValue) return;

        e.Handled = true;

        Point mousePos = e.GetPosition(this.EleContainer);
        Vector delta = mousePos - lastMousePosition.Value;

        var matrix = matrixTransform.Value;

        matrix.TranslatePrepend(delta.X, delta.Y);

        matrixTransform.Matrix = matrix;
        //Trace.WriteLine($"Offset x:{matrix.OffsetX}, y:{matrix.OffsetY}");
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