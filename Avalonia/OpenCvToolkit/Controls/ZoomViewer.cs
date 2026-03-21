using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using System;

namespace OpenCvToolkit.Controls;

[TemplatePart(PART_Root, typeof(Panel))]
[TemplatePart(PART_Container, typeof(Panel))]
//public partial class ImageZoomViewer : TemplatedControl
public partial class ZoomViewer : UserControl
{
    // Template parts
    const string PART_Root = "PART_Root";
    const string PART_Container = "PART_Container";
    Panel? _rootGrid;
    Panel? PART_container;

    // Pan state tracking
    Point _panStartPoint;
    bool _isPanning;


    // Override OnApplyTemplate to get template parts
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // Remove old event handlers
        if (_rootGrid != null)
        {
            _rootGrid.PointerWheelChanged -= OnPointerWheelChanged;
            _rootGrid.PointerPressed -= OnPointerPressed;
            _rootGrid.PointerMoved -= OnPointerMoved;
            _rootGrid.PointerReleased -= OnPointerReleased;
            _rootGrid.PointerCaptureLost -= OnPointerCaptureLost;
        }

        base.OnApplyTemplate(e);

        // Get template parts
        _rootGrid = e.NameScope.Find<Panel>(PART_Root);
        PART_container = e.NameScope.Find<Panel>(PART_Container);

        // Attach event handlers
        if (_rootGrid != null)
        {
            _rootGrid.PointerWheelChanged += OnPointerWheelChanged;
            _rootGrid.PointerPressed += OnPointerPressed;
            _rootGrid.PointerMoved += OnPointerMoved;
            _rootGrid.PointerReleased += OnPointerReleased;
            _rootGrid.PointerCaptureLost += OnPointerCaptureLost;
        }

        // Initialize transform if container exists
        PART_container?.RenderTransform = new MatrixTransform();

    }

    // Mouse pan event handlers
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!EnablePan || PART_container?.RenderTransform == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;

        var point = e.GetCurrentPoint(PART_container);
        _isPanning = true;
        _panStartPoint = point.Position;

        // Capture pointer for smooth dragging
        if (_rootGrid != null)
        {
            _rootGrid.Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(_rootGrid);
        }

        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isPanning || PART_container?.RenderTransform == null) return;

        var point = e.GetCurrentPoint(PART_container);
        var currentPoint = point.Position;

        // Calculate translation
        var delta = currentPoint - _panStartPoint;

        // Apply translation to the start matrix
        var matrix = PART_container.RenderTransform.Value;
        matrix = Matrix.CreateTranslation(delta.X, delta.Y) * matrix;

        // Apply transform 
        if (PART_container != null)
        {
            ApplyTransformWithAnimation(matrix);
        }

        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            EndPanning();
            e.Handled = true;
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_isPanning)
        {
            EndPanning();
        }
    }

    private void EndPanning()
    {
        _isPanning = false;
        _panStartPoint = default;

        _rootGrid?.Cursor = Cursor.Default;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!EnableZoom || PART_container?.RenderTransform == null) return;

        e.Handled = true;

        var point = e.GetCurrentPoint(PART_container);
        double scaleDelta = e.Delta.Y > 0 ? (1 + ZoomStep) : (1 / (1 + ZoomStep));

        // Update transform matrix
        var matrix = PART_container.RenderTransform.Value;
        matrix = ScaleAtPrepend(matrix, scaleDelta, scaleDelta, point.Position.X, point.Position.Y);

        if (Math.Min(matrix.M11, matrix.M22) < MinScale) return;
        if (Math.Max(matrix.M11, matrix.M22) > MaxScale) return;

        // Apply transform with animation
        ApplyTransformWithAnimation(matrix);

        // Update Scale property (will be coerced to MinScale/MaxScale range)
        Scale *= scaleDelta;
    }

    public void ResetZoom()
    {
        Scale = 1.0;

        if (PART_container != null)
        {
            ApplyTransformWithAnimation(Matrix.Identity);
        }
    }

    private static Matrix ScaleAtPrepend(Matrix m, double scaleX, double scaleY, double x, double y)
    {
        m = Matrix.CreateTranslation(x, y) * m;
        m = Matrix.CreateScale(scaleX, scaleY) * m;
        m = Matrix.CreateTranslation(-x, -y) * m;
        return m;
    }

    private void ApplyTransformWithAnimation(Matrix matrix)
    {
        if (PART_container == null) return;

        // Use TransformOperations for smooth animation
        var builder = TransformOperations.CreateBuilder(1);
        builder.AppendMatrix(matrix);
        PART_container.RenderTransform = builder.Build();
    }
}

public partial class ZoomViewer
{
    public static readonly StyledProperty<double> ScaleProperty;
    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public static readonly StyledProperty<double> MinScaleProperty;
    public double MinScale
    {
        get => GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public static readonly StyledProperty<double> MaxScaleProperty;
    public double MaxScale
    {
        get => GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public static readonly StyledProperty<double> ZoomStepProperty;
    public double ZoomStep
    {
        get => GetValue(ZoomStepProperty);
        set => SetValue(ZoomStepProperty, value);
    }

    public static readonly StyledProperty<bool> EnableZoomProperty;
    public bool EnableZoom
    {
        get => GetValue(EnableZoomProperty);
        set => SetValue(EnableZoomProperty, value);
    }

    public static readonly StyledProperty<bool> EnablePanProperty;
    public bool EnablePan
    {
        get => GetValue(EnablePanProperty);
        set => SetValue(EnablePanProperty, value);
    }

    // Static constructor for StyledProperty registration
    static ZoomViewer()
    {
        // Register all StyledProperty instances
        ScaleProperty = AvaloniaProperty.Register<ZoomViewer, double>(
            nameof(Scale),
            defaultValue: 1.0);

        MinScaleProperty = AvaloniaProperty.Register<ZoomViewer, double>(
            nameof(MinScale),
            defaultValue: 0.4);

        MaxScaleProperty = AvaloniaProperty.Register<ZoomViewer, double>(
            nameof(MaxScale),
            defaultValue: 5.0);

        ZoomStepProperty = AvaloniaProperty.Register<ZoomViewer, double>(
            nameof(ZoomStep),
            defaultValue: 0.2);

        EnableZoomProperty = AvaloniaProperty.Register<ZoomViewer, bool>(
            nameof(EnableZoom),
            defaultValue: true);

        EnablePanProperty = AvaloniaProperty.Register<ZoomViewer, bool>(
             nameof(EnablePan),
             defaultValue: true);
    }

}
