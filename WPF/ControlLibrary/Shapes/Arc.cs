using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ControlLibrary.Shapes;

public class Arc : Shape
{
    public Rect Rect
    {
        get { return (Rect)GetValue(RectProperty); }
        set { SetValue(RectProperty, value); }
    }
    public static readonly DependencyProperty RectProperty =
        DependencyProperty.Register(nameof(Rect), typeof(Rect), typeof(Arc), new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public double StartAngle
    {
        get { return (double)GetValue(StartAngleProperty); }
        set { SetValue(StartAngleProperty, value); }
    }
    public static readonly DependencyProperty StartAngleProperty =
        DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(Arc), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public double EndAngle
    {
        get { return (double)GetValue(EndAngleProperty); }
        set { SetValue(EndAngleProperty, value); }
    }
    public static readonly DependencyProperty EndAngleProperty =
        DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(Arc), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    protected override Geometry DefiningGeometry => GetGeometry();

    private Geometry GetGeometry()
    {
        double radius = Math.Min(Rect.Width, Rect.Height);
        if (radius <= 0) return Geometry.Empty;

        if (Math.Abs(StartAngle - EndAngle) == 360)
        {
            EllipseGeometry ellipseGeometry = new();
            ellipseGeometry.RadiusX = radius - StrokeThickness;
            ellipseGeometry.RadiusY = radius - StrokeThickness;
            ellipseGeometry.Freeze();
            return ellipseGeometry;
        }
        else
        {
            EllipseGeometry ellipseGeometry = new();
            ellipseGeometry.RadiusX = radius - StrokeThickness;
            ellipseGeometry.RadiusY = radius - StrokeThickness;
            ellipseGeometry.Center = new Point(radius, radius);
            ellipseGeometry.Freeze();
            return ellipseGeometry;
        }
    }

    private double GetRadian(double angle)
    {
        return angle * Math.PI / 180;
    }
}
