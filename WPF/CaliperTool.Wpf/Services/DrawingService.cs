using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CaliperTool.Wpf.Models;

namespace CaliperTool.Wpf.Services;

/// <summary>
/// 将卡尺工具各要素绘制到 DrawingVisual（无状态纯绘图服务）
/// 所有输入坐标均为画布坐标系（已由 ImageService 做过映射）
/// </summary>
public sealed class DrawingService
{
    // ── 样式常量 ──────────────────────────────────────────────
    private const double EndpointRadius    = 6.0;
    private const double EdgePointRadius   = 4.0;
    private const double AxisLineThickness = 1.5;
    private const double FitLineThickness  = 1.0;
    private const double RoiLineThickness  = 1.0;
    private const double LabelFontSize     = 10.0;

    // ── 画笔（Frozen，跨线程安全）────────────────────────────
    // 蓝色：卡尺端点 + 轴线
    private static readonly SolidColorBrush EndpointFill =
        Freeze(new SolidColorBrush(Color.FromArgb(200, 30, 120, 255)));
    private static readonly Pen EndpointPen =
        Freeze(new Pen(new SolidColorBrush(Colors.White), 1.5));
    private static readonly Pen AxisPen =
        Freeze(new Pen(new SolidColorBrush(Color.FromArgb(220, 30, 140, 255)), AxisLineThickness)
        { DashStyle = DashStyles.Dash });

    // 橙色：ROI 矩形
    private static readonly Pen RoiPen =
        Freeze(new Pen(new SolidColorBrush(Color.FromArgb(200, 255, 165, 0)), RoiLineThickness));
    private static readonly SolidColorBrush RoiInvalidPen_Brush =
        Freeze(new SolidColorBrush(Color.FromArgb(120, 128, 128, 128)));
    private static readonly Pen RoiInvalidPen =
        Freeze(new Pen(RoiInvalidPen_Brush, RoiLineThickness) { DashStyle = DashStyles.Dot });

    // 红色：有效边缘点
    private static readonly SolidColorBrush EdgePointFill =
        Freeze(new SolidColorBrush(Color.FromArgb(230, 255, 50, 50)));
    private static readonly Pen EdgePointPen =
        Freeze(new Pen(new SolidColorBrush(Colors.White), 1.0));

    // 绿色：拟合直线
    private static readonly Pen FitLinePen =
        Freeze(new Pen(new SolidColorBrush(Color.FromArgb(230, 0, 220, 80)), FitLineThickness));

    // 文字
    private static readonly SolidColorBrush LabelBrush =
        Freeze(new SolidColorBrush(Color.FromArgb(220, 255, 220, 60)));
    private static readonly Typeface LabelFace = new("Consolas");

    // ── 圆卡尺专用样式 ────────────────────────────────────────
    private const double CirclePointRadius   = 6.0;
    private const double FitCircleThickness  = 1;
    private const double RefCircleThickness  = 1;
    private const double CircleCenterRadius  = 5.0;

    // 青色：圆卡尺定义点
    private static readonly SolidColorBrush CirclePointFill =
        Freeze(new SolidColorBrush(Color.FromArgb(200, 0, 200, 200)));
    private static readonly Pen CirclePointPen =
        Freeze(new Pen(new SolidColorBrush(Colors.White), 1.5));

    // 蓝色虚线：参考圆弧
    private static readonly Pen RefCirclePen =
        Freeze(new Pen(new SolidColorBrush(Color.FromArgb(180, 30, 140, 255)), RefCircleThickness)
        { DashStyle = DashStyles.Dash });

    // 绿色：拟合圆 + 圆心
    private static readonly Pen FitCirclePen =
        Freeze(new Pen(new SolidColorBrush(Color.FromArgb(230, 0, 220, 80)), FitCircleThickness));
    private static readonly SolidColorBrush FitCenterFill =
        Freeze(new SolidColorBrush(Color.FromArgb(230, 0, 220, 80)));
    private static readonly Pen FitCenterCrossPen =
        Freeze(new Pen(new SolidColorBrush(Color.FromArgb(230, 0, 220, 80)), 1.5));

    // ── 公共绘制方法 ──────────────────────────────────────────

    /// <summary>绘制卡尺轴线（蓝色虚线）+ 两个端点（蓝色实心圆）</summary>
    public DrawingVisual DrawCaliperAxis(
        Point canvasP1, Point canvasP2,
        bool p1Only = false)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        // 端点1
        dc.DrawEllipse(EndpointFill, EndpointPen, canvasP1, EndpointRadius, EndpointRadius);
        DrawLabel(dc, "P1", canvasP1, 8, -16);

        if (!p1Only)
        {
            // 轴线
            dc.DrawLine(AxisPen, canvasP1, canvasP2);
            // 端点2
            dc.DrawEllipse(EndpointFill, EndpointPen, canvasP2, EndpointRadius, EndpointRadius);
            DrawLabel(dc, "P2", canvasP2, 8, -16);
        }

        return visual;
    }

    /// <summary>绘制所有 ROI 矩形框（橙色）</summary>
    public DrawingVisual DrawRois(
        CaliperLine caliper,
        ImageService imageService,
        double canvasW, double canvasH)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        for (int i = 0; i < caliper.RoiCount; i++)
        {
            var (tl, tr, br, bl) = caliper.GetRoiCorners(i);

            // 转换为画布坐标
            var ctl = imageService.ImageToCanvasCoordinate(tl, canvasW, canvasH);
            var ctr = imageService.ImageToCanvasCoordinate(tr, canvasW, canvasH);
            var cbr = imageService.ImageToCanvasCoordinate(br, canvasW, canvasH);
            var cbl = imageService.ImageToCanvasCoordinate(bl, canvasW, canvasH);

            // 绘制四边形（PathGeometry）
            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = ctl, IsClosed = true };
            fig.Segments.Add(new LineSegment(ctr, true));
            fig.Segments.Add(new LineSegment(cbr, true));
            fig.Segments.Add(new LineSegment(cbl, true));
            geo.Figures.Add(fig);

            dc.DrawGeometry(null, RoiPen, geo);
        }

        return visual;
    }

    /// <summary>绘制边缘检测结果：有效点（红色实心圆）+ 无效 ROI 中心（灰色叉）</summary>
    public DrawingVisual DrawEdgePoints(
        IReadOnlyList<RoiResult> results,
        ImageService imageService,
        double canvasW, double canvasH,
        double dpi)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        foreach (var r in results)
        {
            if (r.IsValid)
            {
                var cp = imageService.ImageToCanvasCoordinate(r.EdgePoint!.Value, canvasW, canvasH);
                dc.DrawEllipse(EdgePointFill, EdgePointPen, cp, EdgePointRadius, EdgePointRadius);
            }
            else
            {
                // 无效：在 ROI 中心画灰色叉
                var cc = imageService.ImageToCanvasCoordinate(r.Center, canvasW, canvasH);
                var gray = RoiInvalidPen;
                const double s = 5;
                dc.DrawLine(gray, new Point(cc.X - s, cc.Y - s), new Point(cc.X + s, cc.Y + s));
                dc.DrawLine(gray, new Point(cc.X + s, cc.Y - s), new Point(cc.X - s, cc.Y + s));
            }
        }

        return visual;
    }

    /// <summary>绘制拟合直线（绿色），延伸到图像边界</summary>
    public DrawingVisual? DrawFittedLine(
        LineResult result,
        ImageService imageService,
        double canvasW, double canvasH)
    {
        if (!imageService.HasImage) return null;

        var (imgP1, imgP2) = ComputeLineEndpointsInImage(
            result, imageService.ImageWidth, imageService.ImageHeight);

        var cp1 = imageService.ImageToCanvasCoordinate(imgP1, canvasW, canvasH);
        var cp2 = imageService.ImageToCanvasCoordinate(imgP2, canvasW, canvasH);

        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();
        dc.DrawLine(FitLinePen, cp1, cp2);
        return visual;
    }

    // ── 圆卡尺公共绘制方法 ────────────────────────────────────

    /// <summary>
    /// 绘制圆卡尺三个定义点（青色）+ 预览参考圆（蓝色虚线）
    /// </summary>
    /// <param name="canvasP1/P2/P3">已转换为画布坐标的三点（null 表示尚未设置）</param>
    /// <param name="previewCx/Cy/R">三点构成的预览圆圆心和半径（画布坐标）</param>
    /// <param name="hasPreview">是否显示预览圆（三点均设置后为 true）</param>
    public DrawingVisual DrawCircleDefinition(
        Point? canvasP1, Point? canvasP2, Point? canvasP3,
        double previewCx, double previewCy, double previewR,
        bool hasPreview)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        // 预览参考圆（蓝色虚线，三点全部设置后才显示）
        if (hasPreview && previewR > 0)
            DrawCircleArc(dc, RefCirclePen, previewCx, previewCy, previewR, 0, 360);

        // 三个定义点
        if (canvasP1.HasValue)
        {
            dc.DrawEllipse(CirclePointFill, CirclePointPen,
                canvasP1.Value, CirclePointRadius, CirclePointRadius);
            DrawLabel(dc, "P1", canvasP1.Value, 8, -16);
        }
        if (canvasP2.HasValue)
        {
            dc.DrawEllipse(CirclePointFill, CirclePointPen,
                canvasP2.Value, CirclePointRadius, CirclePointRadius);
            DrawLabel(dc, "P2", canvasP2.Value, 8, -16);
        }
        if (canvasP3.HasValue)
        {
            dc.DrawEllipse(CirclePointFill, CirclePointPen,
                canvasP3.Value, CirclePointRadius, CirclePointRadius);
            DrawLabel(dc, "P3", canvasP3.Value, 8, -16);
        }

        return visual;
    }

    /// <summary>
    /// 绘制圆卡尺 ROI 分布：参考圆弧 + N 个径向矩形框（橙色）
    /// </summary>
    public DrawingVisual DrawCircleRois(
        double imgCx, double imgCy, double imgR,
        int roiCount, int roiHalfWidth,
        double startAngleDeg, double sweepAngleDeg,
        ImageService imageService,
        double canvasW, double canvasH)
    {
        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        // 将圆心和两个半径端点映射到画布，计算画布半径
        var cCenter = imageService.ImageToCanvasCoordinate(
            new Point(imgCx, imgCy), canvasW, canvasH);
        var cEdge = imageService.ImageToCanvasCoordinate(
            new Point(imgCx + imgR, imgCy), canvasW, canvasH);
        double canvasR = Math.Abs(cEdge.X - cCenter.X);

        // 参考圆弧（蓝色虚线）
        DrawCircleArc(dc, RefCirclePen,
            cCenter.X, cCenter.Y, canvasR,
            startAngleDeg, sweepAngleDeg);

        // 各 ROI 矩形框
        double axisStep = roiCount > 1
            ? sweepAngleDeg / (roiCount - 1)
            : sweepAngleDeg;
        double halfAlong = axisStep * 0.45; // 沿弧方向的半张角（度）

        for (int i = 0; i < roiCount; i++)
        {
            double fraction  = roiCount == 1 ? 0.5 : (double)i / (roiCount - 1);
            double angleDeg  = startAngleDeg + fraction * sweepAngleDeg;
            double angleRad  = angleDeg * Math.PI / 180.0;

            // ROI 中心（图像坐标）
            double rcx = imgCx + imgR * Math.Cos(angleRad);
            double rcy = imgCy + imgR * Math.Sin(angleRad);

            // 径向方向和切向方向（图像坐标系）
            double radX = Math.Cos(angleRad);   // 径向（外）
            double radY = Math.Sin(angleRad);
            double tanX = -Math.Sin(angleRad);  // 切向（逆时针）
            double tanY =  Math.Cos(angleRad);

            // ROI 的四个角点（图像坐标）
            // 沿切向展开 halfAlong°对应的弧长（近似 R * halfAlong_rad）
            double halfAlongPx = imgR * halfAlong * Math.PI / 180.0;
            var imgTL = new Point(rcx - tanX * halfAlongPx - radX * roiHalfWidth,
                                  rcy - tanY * halfAlongPx - radY * roiHalfWidth);
            var imgTR = new Point(rcx + tanX * halfAlongPx - radX * roiHalfWidth,
                                  rcy + tanY * halfAlongPx - radY * roiHalfWidth);
            var imgBR = new Point(rcx + tanX * halfAlongPx + radX * roiHalfWidth,
                                  rcy + tanY * halfAlongPx + radY * roiHalfWidth);
            var imgBL = new Point(rcx - tanX * halfAlongPx + radX * roiHalfWidth,
                                  rcy - tanY * halfAlongPx + radY * roiHalfWidth);

            // 转换为画布坐标并绘制四边形
            var ctl = imageService.ImageToCanvasCoordinate(imgTL, canvasW, canvasH);
            var ctr = imageService.ImageToCanvasCoordinate(imgTR, canvasW, canvasH);
            var cbr = imageService.ImageToCanvasCoordinate(imgBR, canvasW, canvasH);
            var cbl = imageService.ImageToCanvasCoordinate(imgBL, canvasW, canvasH);

            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = ctl, IsClosed = true };
            fig.Segments.Add(new LineSegment(ctr, true));
            fig.Segments.Add(new LineSegment(cbr, true));
            fig.Segments.Add(new LineSegment(cbl, true));
            geo.Figures.Add(fig);
            dc.DrawGeometry(null, RoiPen, geo);
        }

        return visual;
    }

    /// <summary>
    /// 绘制拟合圆（绿色实线）+ 圆心标记（十字 + 实心圆点）
    /// </summary>
    public DrawingVisual? DrawFittedCircle(
        CircleResult result,
        ImageService imageService,
        double canvasW, double canvasH)
    {
        if (!imageService.HasImage) return null;

        var cCenter = imageService.ImageToCanvasCoordinate(
            new Point(result.CenterX, result.CenterY), canvasW, canvasH);
        var cEdge = imageService.ImageToCanvasCoordinate(
            new Point(result.CenterX + result.Radius, result.CenterY), canvasW, canvasH);
        double canvasR = Math.Abs(cEdge.X - cCenter.X);

        var visual = new DrawingVisual();
        using var dc = visual.RenderOpen();

        // 拟合圆（绿色实线）
        dc.DrawEllipse(null, FitCirclePen, cCenter, canvasR, canvasR);

        // 圆心十字线
        const double crossLen = 12;
        dc.DrawLine(FitCenterCrossPen,
            new Point(cCenter.X - crossLen, cCenter.Y),
            new Point(cCenter.X + crossLen, cCenter.Y));
        dc.DrawLine(FitCenterCrossPen,
            new Point(cCenter.X, cCenter.Y - crossLen),
            new Point(cCenter.X, cCenter.Y + crossLen));

        // 圆心实心圆点
        dc.DrawEllipse(FitCenterFill, null, cCenter, CircleCenterRadius, CircleCenterRadius);

        return visual;
    }

    // ── 私有辅助 ──────────────────────────────────────────────

    /// <summary>
    /// 在 DrawingContext 上绘制圆弧（用 PathGeometry ArcSegment 实现）
    /// sweepAngle = 360 时绘制完整圆
    /// </summary>
    private static void DrawCircleArc(
        DrawingContext dc, Pen pen,
        double cx, double cy, double r,
        double startDeg, double sweepDeg)
    {
        if (r <= 0) return;

        // 360° 整圆直接用 DrawEllipse，避免 ArcSegment 360°退化问题
        if (Math.Abs(sweepDeg - 360) < 0.1)
        {
            dc.DrawEllipse(null, pen, new Point(cx, cy), r, r);
            return;
        }

        double startRad = startDeg * Math.PI / 180.0;
        double endRad   = (startDeg + sweepDeg) * Math.PI / 180.0;

        var startPt = new Point(cx + r * Math.Cos(startRad), cy + r * Math.Sin(startRad));
        var endPt   = new Point(cx + r * Math.Cos(endRad),   cy + r * Math.Sin(endRad));

        var geo = new PathGeometry();
        var fig = new PathFigure { StartPoint = startPt };
        fig.Segments.Add(new ArcSegment(
            endPt,
            new Size(r, r),
            rotationAngle: 0,
            isLargeArc: sweepDeg > 180,
            sweepDirection: SweepDirection.Clockwise,
            isStroked: true));
        geo.Figures.Add(fig);
        dc.DrawGeometry(null, pen, geo);
    }

    private static (Point p1, Point p2) ComputeLineEndpointsInImage(
        LineResult r, int imgW, int imgH)
    {
        var tValues = new List<double>(4);

        if (Math.Abs(r.Vx) > 1e-9)
        {
            tValues.Add((0 - r.X0)            / r.Vx);
            tValues.Add((imgW - 1 - r.X0)     / r.Vx);
        }
        if (Math.Abs(r.Vy) > 1e-9)
        {
            tValues.Add((0 - r.Y0)            / r.Vy);
            tValues.Add((imgH - 1 - r.Y0)     / r.Vy);
        }

        var valid = tValues
            .Select(t => new Point(r.X0 + t * r.Vx, r.Y0 + t * r.Vy))
            .Where(p => p.X >= -0.5 && p.X <= imgW - 0.5 &&
                        p.Y >= -0.5 && p.Y <= imgH - 0.5)
            .OrderBy(p => p.X)
            .ToList();

        if (valid.Count >= 2) return (valid.First(), valid.Last());

        double len = Math.Max(imgW, imgH);
        return (new Point(r.X0 - r.Vx * len, r.Y0 - r.Vy * len),
                new Point(r.X0 + r.Vx * len, r.Y0 + r.Vy * len));
    }

    private void DrawLabel(DrawingContext dc, string text, Point anchor, double ox, double oy)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, LabelFace, LabelFontSize, LabelBrush, 1.0);
        dc.DrawText(ft, new Point(anchor.X + ox, anchor.Y + oy));
    }

    private static T Freeze<T>(T f) where T : Freezable { f.Freeze(); return f; }
}
