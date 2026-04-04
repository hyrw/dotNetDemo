using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CaliperTool.Wpf.Models;
using CaliperTool.Wpf.Services;

namespace CaliperTool.Wpf.Controls;

/// <summary>
/// 交互式图像画布控件
/// 三阶段点击交互：
///   阶段1 – 等待 P1（显示十字光标）
///   阶段2 – 等待 P2，同时实时预览轴线
///   阶段3 – 卡尺就绪，不再响应点击（ROI 参数由外部调整后触发刷新）
/// </summary>
public partial class ImageCanvas : System.Windows.Controls.UserControl
{
    // ── 依赖属性 ──────────────────────────────────────────────

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource), typeof(BitmapSource), typeof(ImageCanvas),
            new PropertyMetadata(null, OnImageSourceChanged));

    public BitmapSource? ImageSource
    {
        get => (BitmapSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    // ── 对外事件 ──────────────────────────────────────────────

    /// <summary>用户完成一次有效点击（图像坐标，亚像素精度）</summary>
    public event Action<Point>? CanvasPointClicked;

    // ── 内部服务 ──────────────────────────────────────────────

    public ImageService ImageService { get; }   // 供 ViewModel 访问图像尺寸
    private readonly DrawingService _drawing;

    // ── 构造 / 初始化 ─────────────────────────────────────────

    public ImageCanvas()
    {
        InitializeComponent();
        ImageService = new ImageService();
        _drawing     = new DrawingService();
        Loaded += (_, _) =>
        {
            PART_DrawingHost.MouseLeftButtonDown += OnMouseDown;
            PART_DrawingHost.MouseMove           += OnMouseMove;
            PART_DrawingHost.MouseLeave          += OnMouseLeave;
        };
    }

    // ── 图像加载（外部调用）──────────────────────────────────

    /// <summary>加载图像文件，成功后返回 BitmapSource，同时更新依赖属性</summary>
    public BitmapSource? LoadImageFromFile(string filePath)
    {
        try
        {
            var bmp = ImageService.LoadImage(filePath);
            ImageSource = bmp;
            return bmp;
        }
        catch { return null; }
    }

    // ── 绘制 API（由外部驱动刷新）────────────────────────────

    /// <summary>绘制卡尺轴线 + 端点（蓝色）。p1Only=true 时仅绘制 P1</summary>
    public void RefreshAxis(Point imgP1, Point imgP2, bool p1Only = false)
    {
        var cp1 = ToCanvas(imgP1);
        var cp2 = ToCanvas(imgP2);
        PART_DrawingHost.SetAxisVisual(_drawing.DrawCaliperAxis(cp1, cp2, p1Only));
    }

    /// <summary>绘制所有 ROI 矩形框（橙色）</summary>
    public void RefreshRois(CaliperLine caliper)
    {
        double w = PART_DrawingHost.ActualWidth;
        double h = PART_DrawingHost.ActualHeight;
        PART_DrawingHost.SetRoiVisual(_drawing.DrawRois(caliper, ImageService, w, h));
    }

    /// <summary>绘制边缘检测结果（红色点 + 无效叉）</summary>
    public void RefreshEdgePoints(IReadOnlyList<RoiResult> results)
    {
        double w   = PART_DrawingHost.ActualWidth;
        double h   = PART_DrawingHost.ActualHeight;
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        PART_DrawingHost.SetEdgeVisual(
            _drawing.DrawEdgePoints(results, ImageService, w, h, dpi));
    }

    /// <summary>绘制拟合直线（绿色）</summary>
    public void RefreshFitLine(LineResult? result)
    {
        if (result is null) { PART_DrawingHost.SetFitLineVisual(null); return; }
        double w = PART_DrawingHost.ActualWidth;
        double h = PART_DrawingHost.ActualHeight;
        PART_DrawingHost.SetFitLineVisual(_drawing.DrawFittedLine(result, ImageService, w, h));
    }

    /// <summary>清空所有绘制层</summary>
    public void ClearAll() => PART_DrawingHost.ClearAll();

    /// <summary>仅清空检测结果（边缘点 + 拟合线），保留轴线和 ROI 框</summary>
    public void ClearResults() => PART_DrawingHost.ClearResults();

    // ── 圆卡尺绘制 API ────────────────────────────────────────

    /// <summary>
    /// 刷新圆卡尺三个定义点（青色）+ 预览参考圆（蓝色虚线）
    /// null 表示该点尚未设置
    /// </summary>
    public void RefreshCircleAxis(
        Point? imgP1, Point? imgP2, Point? imgP3,
        double cx, double cy, double r,
        bool hasPreview)
    {
        double w = PART_DrawingHost.ActualWidth;
        double h = PART_DrawingHost.ActualHeight;

        Point? cp1 = imgP1.HasValue ? ToCanvas(imgP1.Value) : null;
        Point? cp2 = imgP2.HasValue ? ToCanvas(imgP2.Value) : null;
        Point? cp3 = imgP3.HasValue ? ToCanvas(imgP3.Value) : null;

        // 将图像坐标圆心/半径转为画布坐标
        var cCenter = ImageService.ImageToCanvasCoordinate(new Point(cx, cy), w, h);
        var cEdge   = ImageService.ImageToCanvasCoordinate(new Point(cx + r, cy), w, h);
        double canvasR = Math.Abs(cEdge.X - cCenter.X);

        PART_DrawingHost.SetAxisVisual(
            _drawing.DrawCircleDefinition(cp1, cp2, cp3,
                cCenter.X, cCenter.Y, canvasR, hasPreview));
    }

    /// <summary>刷新圆卡尺 ROI 分布（橙色径向矩形框 + 参考圆弧）</summary>
    public void RefreshCircleRois(
        double cx, double cy, double r,
        int roiCount, int roiHalfWidth,
        double startAngle, double sweepAngle)
    {
        double w = PART_DrawingHost.ActualWidth;
        double h = PART_DrawingHost.ActualHeight;
        PART_DrawingHost.SetRoiVisual(
            _drawing.DrawCircleRois(cx, cy, r,
                roiCount, roiHalfWidth,
                startAngle, sweepAngle,
                ImageService, w, h));
    }

    /// <summary>刷新拟合圆（绿色实线 + 圆心标记）</summary>
    public void RefreshFittedCircle(CircleResult? result)
    {
        if (result is null) { PART_DrawingHost.SetFitLineVisual(null); return; }
        double w = PART_DrawingHost.ActualWidth;
        double h = PART_DrawingHost.ActualHeight;
        PART_DrawingHost.SetFitLineVisual(
            _drawing.DrawFittedCircle(result, ImageService, w, h));
    }

    // ── 依赖属性回调 ──────────────────────────────────────────

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var c = (ImageCanvas)d;
        c.PART_Image.Source = e.NewValue as BitmapSource;
        c.PART_Hint.Visibility = e.NewValue is null ? Visibility.Visible : Visibility.Collapsed;
        c.PART_DrawingHost.ClearAll();
    }

    // ── 鼠标事件 ──────────────────────────────────────────────

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!ImageService.HasImage) return;
        var imgPt = ToImage(e.GetPosition(PART_DrawingHost));
        if (!ImageService.IsInImageBounds(imgPt)) return;
        CanvasPointClicked?.Invoke(imgPt);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!ImageService.HasImage) { PART_CoordBorder.Visibility = Visibility.Hidden; return; }
        var imgPt = ToImage(e.GetPosition(PART_DrawingHost));
        if (ImageService.IsInImageBounds(imgPt))
        {
            PART_CoordText.Text = $"X: {imgPt.X:F2}  Y: {imgPt.Y:F2}";
            PART_CoordBorder.Visibility = Visibility.Visible;
        }
        else PART_CoordBorder.Visibility = Visibility.Hidden;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e) =>
        PART_CoordBorder.Visibility = Visibility.Hidden;

    // ── 坐标转换辅助 ──────────────────────────────────────────

    private Point ToImage(Point canvasPt) =>
        ImageService.CanvasToImageCoordinate(
            canvasPt,
            PART_DrawingHost.ActualWidth,
            PART_DrawingHost.ActualHeight);

    private Point ToCanvas(Point imgPt) =>
        ImageService.ImageToCanvasCoordinate(
            imgPt,
            PART_DrawingHost.ActualWidth,
            PART_DrawingHost.ActualHeight);
}
