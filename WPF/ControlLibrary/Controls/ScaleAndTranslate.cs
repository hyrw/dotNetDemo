using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ControlLibrary.Controls;

[TemplatePart(Name = PART_Container, Type = typeof(Border))]
[TemplatePart(Name = PART_MatrixTransform, Type = typeof(MatrixTransform))]
public class ScaleAndTranslate : ContentControl
{
    const string PART_Container = "PART_Container";
    const string PART_MatrixTransform = "PART_MatrixTransform";

    Border? eleContainer;
    MatrixTransform? matrixTransform;

    private const double ZoomFactor = 1.2;
    private const double MinZoom = 0.2;
    private const double MaxZoom = 5.0;

    private Point? lastMousePosition;
    private bool isDragging;

    static ScaleAndTranslate()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ScaleAndTranslate), new FrameworkPropertyMetadata(typeof(ScaleAndTranslate)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild(PART_Container) is not Border border) return;
        if (GetTemplateChild(PART_MatrixTransform) is not MatrixTransform matrix) return;
        eleContainer = border;
        matrixTransform = matrix;

        eleContainer.MouseWheel -= Grid_MouseWheel;
        eleContainer.MouseWheel += Grid_MouseWheel;

        eleContainer.MouseMove -= Grid_MouseMove;
        eleContainer.MouseMove += Grid_MouseMove;

        eleContainer.MouseLeftButtonDown -= Grid_MouseLeftButtonDown;
        eleContainer.MouseLeftButtonDown += Grid_MouseLeftButtonDown;

        eleContainer.MouseLeftButtonUp -= Grid_MouseLeftButtonUp;
        eleContainer.MouseLeftButtonUp += Grid_MouseLeftButtonUp;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.matrixTransform != null)
        {
            this.matrixTransform.Matrix = Matrix.Identity;
        }
    }

    private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (matrixTransform is null) return;
        e.Handled = true;

        Point point = e.GetPosition(this.eleContainer);

        var matrix = this.matrixTransform.Value;

        double delta = e.Delta > 0 ? ZoomFactor : 1 / ZoomFactor;

        // 方式1 使用MatrixTransform
        matrix.ScaleAtPrepend(delta, delta, point.X, point.Y);
        //this.MatrixStoryboard.Begin();
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
        if (this.matrixTransform is null) return;
        if (!isDragging || !lastMousePosition.HasValue) return;

        e.Handled = true;

        Point mousePos = e.GetPosition(this.eleContainer);
        Vector delta = mousePos - lastMousePosition.Value;

        var matrix = this.matrixTransform.Value;

        matrix.Translate(delta.X, delta.Y);

        this.matrixTransform.Matrix = matrix;
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement ele) return;

        ele.CaptureMouse();
        lastMousePosition = e.GetPosition(this.eleContainer);
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
