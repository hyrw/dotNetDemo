using WpfPoint = System.Windows.Point;

namespace CaliperTool.Wpf.Models;

/// <summary>
/// 直线卡尺工具的完整定义：由两个端点构成轴线，沿轴线均匀分布 N 个垂直搜索 ROI
/// </summary>
public sealed class CaliperLine
{
    // ── 端点（图像坐标系，亚像素精度）────────────────────────
    public WpfPoint P1 { get; }
    public WpfPoint P2 { get; }

    // ── ROI 参数 ──────────────────────────────────────────────
    /// <summary>沿轴线均匀分布的 ROI 数量</summary>
    public int RoiCount { get; }

    /// <summary>每个 ROI 在垂直方向（搜索方向）的半宽（像素），总宽 = 2*HalfWidth</summary>
    public int RoiHalfWidth { get; }

    // ── 计算属性 ──────────────────────────────────────────────

    /// <summary>P1→P2 的轴线长度（像素）</summary>
    public double Length
    {
        get
        {
            double dx = P2.X - P1.X;
            double dy = P2.Y - P1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>轴线方向单位向量（P1 → P2）</summary>
    public (double Dx, double Dy) AxisDirection
    {
        get
        {
            double len = Length;
            if (len < 1e-9) return (1, 0);
            return ((P2.X - P1.X) / len, (P2.Y - P1.Y) / len);
        }
    }

    /// <summary>垂直于轴线的单位向量（顺时针旋转 90°）</summary>
    public (double Dx, double Dy) PerpDirection
    {
        get
        {
            var (ax, ay) = AxisDirection;
            return (-ay, ax); // 旋转 90°
        }
    }

    /// <summary>轴线角度（相对 x 轴正方向，度）</summary>
    public double AngleDegrees
    {
        get
        {
            var (dx, dy) = AxisDirection;
            return Math.Atan2(dy, dx) * 180.0 / Math.PI;
        }
    }

    public CaliperLine(WpfPoint p1, WpfPoint p2, int roiCount, int roiHalfWidth)
    {
        P1           = p1;
        P2           = p2;
        RoiCount     = Math.Max(2, roiCount);
        RoiHalfWidth = Math.Max(5, roiHalfWidth);
    }

    /// <summary>
    /// 计算所有 ROI 的中心点（图像坐标），沿轴线均匀分布
    /// </summary>
    public IReadOnlyList<WpfPoint> GetRoiCenters()
    {
        var centers = new List<WpfPoint>(RoiCount);
        for (int i = 0; i < RoiCount; i++)
        {
            // t 从 0 到 1 均匀采样
            double t = RoiCount == 1 ? 0.5 : i / (double)(RoiCount - 1);
            double cx = P1.X + (P2.X - P1.X) * t;
            double cy = P1.Y + (P2.Y - P1.Y) * t;
            centers.Add(new WpfPoint(cx, cy));
        }
        return centers;
    }

    /// <summary>
    /// 获取第 i 个 ROI 的四个角点（图像坐标），用于绘制矩形框
    /// ROI 矩形：以 center 为中心，沿轴线方向宽度为轴线步长，垂直方向为 RoiHalfWidth*2
    /// </summary>
    public (WpfPoint TopLeft, WpfPoint TopRight, WpfPoint BottomRight, WpfPoint BottomLeft)
        GetRoiCorners(int roiIndex)
    {
        var centers = GetRoiCenters();
        var center  = centers[roiIndex];
        var (ax, ay) = AxisDirection;
        var (px, py) = PerpDirection;

        // ROI 沿轴线方向的半长 = 相邻 ROI 间距的一半
        double axisStep  = RoiCount > 1 ? Length / (RoiCount - 1) : Length;
        double halfAlong = axisStep * 0.45; // 略小于间距，留缝隙

        // 四个角点
        var tl = new WpfPoint(center.X - ax * halfAlong - px * RoiHalfWidth,
                              center.Y - ay * halfAlong - py * RoiHalfWidth);
        var tr = new WpfPoint(center.X + ax * halfAlong - px * RoiHalfWidth,
                              center.Y + ay * halfAlong - py * RoiHalfWidth);
        var br = new WpfPoint(center.X + ax * halfAlong + px * RoiHalfWidth,
                              center.Y + ay * halfAlong + py * RoiHalfWidth);
        var bl = new WpfPoint(center.X - ax * halfAlong + px * RoiHalfWidth,
                              center.Y - ay * halfAlong + py * RoiHalfWidth);

        return (tl, tr, br, bl);
    }
}
