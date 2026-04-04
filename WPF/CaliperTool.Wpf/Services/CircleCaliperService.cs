using CaliperTool.Wpf.Models;
using OpenCvSharp;
using WpfPoint = System.Windows.Point;

namespace CaliperTool.Wpf.Services;

/// <summary>
/// 圆卡尺核心服务：
///   1. 三点解析求圆（精确解）
///   2. 沿圆周均匀分布 ROI，在径向方向梯度检测边缘点（亚像素）
///   3. Kasa 代数最小二乘圆拟合
/// </summary>
public sealed class CircleCaliperService
{
    // ── 公共 API ──────────────────────────────────────────────

    /// <summary>
    /// 由三点精确计算外接圆（解析几何法）
    /// </summary>
    /// <returns>(cx, cy, r)；三点共线时返回 null</returns>
    public static (double Cx, double Cy, double R)? ThreePointsToCircle(
        WpfPoint p1, WpfPoint p2, WpfPoint p3)
    {
        // 用垂直平分线交点法求圆心
        // 线段 P1P2 的中点和斜率
        double ax = p1.X, ay = p1.Y;
        double bx = p2.X, by = p2.Y;
        double cx = p3.X, cy = p3.Y;

        // 行列式判三点共线
        double det = (bx - ax) * (cy - ay) - (cx - ax) * (by - ay);
        if (Math.Abs(det) < 1e-9) return null; // 三点共线

        // 利用外接圆公式（等效最小二乘单步解）
        double D = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
        if (Math.Abs(D) < 1e-9) return null;

        double ux = ((ax * ax + ay * ay) * (by - cy)
                   + (bx * bx + by * by) * (cy - ay)
                   + (cx * cx + cy * cy) * (ay - by)) / D;
        double uy = ((ax * ax + ay * ay) * (cx - bx)
                   + (bx * bx + by * by) * (ax - cx)
                   + (cx * cx + cy * cy) * (bx - ax)) / D;

        double dx = ax - ux, dy = ay - uy;
        double r  = Math.Sqrt(dx * dx + dy * dy);

        return (ux, uy, r);
    }

    /// <summary>
    /// 沿圆周均匀分布 roiCount 个径向 ROI，对每个 ROI 做梯度检测
    /// </summary>
    /// <param name="image">源图像</param>
    /// <param name="cx">参考圆心 X（图像坐标）</param>
    /// <param name="cy">参考圆心 Y（图像坐标）</param>
    /// <param name="r">参考半径</param>
    /// <param name="roiCount">ROI 数量</param>
    /// <param name="roiHalfWidth">ROI 径向半宽（像素）</param>
    /// <param name="startAngleDeg">搜索起始角度（度，0=右方）</param>
    /// <param name="sweepAngleDeg">搜索扫描范围（度，360=全圆）</param>
    public IReadOnlyList<RoiResult> DetectEdgePoints(
        Mat image,
        double cx, double cy, double r,
        int roiCount, int roiHalfWidth,
        double startAngleDeg, double sweepAngleDeg)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Empty()) throw new ArgumentException("图像为空。");

        using var gray = ToGray(image);
        var results = new List<RoiResult>(roiCount);

        for (int i = 0; i < roiCount; i++)
        {
            // 当前 ROI 的角度（弧度）
            double fraction = roiCount == 1 ? 0.5 : (double)i / (roiCount - 1);
            double angleDeg = startAngleDeg + fraction * sweepAngleDeg;
            double angleRad = angleDeg * Math.PI / 180.0;

            // ROI 中心：圆周上的点
            double rcx = cx + r * Math.Cos(angleRad);
            double rcy = cy + r * Math.Sin(angleRad);
            var center = new WpfPoint(rcx, rcy);

            // 径向方向（从圆心向外）
            double px = Math.Cos(angleRad);
            double py = Math.Sin(angleRad);

            var detection = DetectSingleRoi(gray, center, px, py, roiHalfWidth);
            results.Add(new RoiResult(i, center, detection.EdgePoint, detection.MaxGradient));
        }

        return results;
    }

    /// <summary>
    /// Kasa 代数最小二乘圆拟合
    /// 圆方程：x² + y² + Dx + Ey + F = 0
    /// 圆心：(-D/2, -E/2)，半径：sqrt(cx²+cy²-F)
    /// </summary>
    /// <returns>圆拟合结果；有效点不足3个时返回 null</returns>
    public static CircleResult? FitCircle(IReadOnlyList<RoiResult> roiResults)
    {
        var pts = roiResults
            .Where(r => r.IsValid)
            .Select(r => r.EdgePoint!.Value)
            .ToList();

        if (pts.Count < 3) return null;

        int n = pts.Count;

        // 构建超定方程组 A·[D,E,F]ᵀ = b
        // 每行：[xi, yi, 1] · [D,E,F]ᵀ = -(xi²+yi²)
        // 正规方程：AᵀA · x = Aᵀb

        double sumX  = 0, sumY  = 0, sumXX = 0, sumYY = 0;
        double sumXY = 0, sumX3 = 0, sumY3 = 0;
        double sumX2Y = 0, sumXY2 = 0;

        foreach (var p in pts)
        {
            double x = p.X, y = p.Y;
            double x2 = x * x, y2 = y * y;
            sumX   += x;
            sumY   += y;
            sumXX  += x2;
            sumYY  += y2;
            sumXY  += x * y;
            sumX3  += x2 * x;
            sumY3  += y2 * y;
            sumX2Y += x2 * y;
            sumXY2 += x * y2;
        }

        // 3×3 正规方程：
        // [ ΣXX  ΣXY  ΣX ] [D]   [ -ΣX³  - ΣXY²  ]
        // [ ΣXY  ΣYY  ΣY ] [E] = [ -ΣX²Y - ΣY³    ]
        // [ ΣX   ΣY   N  ] [F]   [ -ΣXX  - ΣYY    ]

        double[,] A = {
            { sumXX, sumXY, sumX },
            { sumXY, sumYY, sumY },
            { sumX,  sumY,  n    }
        };
        double[] b = {
            -(sumX3  + sumXY2),
            -(sumX2Y + sumY3),
            -(sumXX  + sumYY)
        };

        // Gaussian 消元求解 3×3 线性方程组
        if (!SolveLinear3(A, b, out double D, out double E, out double F))
            return null;

        double fitCx = -D / 2.0;
        double fitCy = -E / 2.0;
        double r2    = fitCx * fitCx + fitCy * fitCy - F;
        if (r2 <= 0) return null;
        double fitR = Math.Sqrt(r2);

        // 计算误差指标
        double mse = 0, maxDev = 0;
        foreach (var p in pts)
        {
            double dx   = p.X - fitCx;
            double dy   = p.Y - fitCy;
            double dist = Math.Abs(Math.Sqrt(dx * dx + dy * dy) - fitR);
            mse    += dist * dist;
            if (dist > maxDev) maxDev = dist;
        }
        mse /= n;

        return new CircleResult
        {
            CenterX          = fitCx,
            CenterY          = fitCy,
            Radius           = fitR,
            MeanSquaredError = mse,
            MaxDeviation     = maxDev,
            PointCount       = n
        };
    }

    // ── 私有辅助 ──────────────────────────────────────────────

    private record struct SingleRoiDetection(WpfPoint? EdgePoint, double MaxGradient);

    /// <summary>
    /// 沿 (px, py) 方向从 center 出发提取灰度剖面，梯度峰值 + 亚像素插值
    /// （与 CaliperService 逻辑完全一致，此处内联以保持服务独立性）
    /// </summary>
    private static SingleRoiDetection DetectSingleRoi(
        Mat gray, WpfPoint center,
        double px, double py, int halfWidth)
    {
        int total = halfWidth * 2 + 1;
        var profile = new double[total];

        for (int k = -halfWidth; k <= halfWidth; k++)
        {
            double val = BilinearSample(gray,
                center.X + k * px,
                center.Y + k * py);
            if (double.IsNaN(val)) return new SingleRoiDetection(null, 0);
            profile[k + halfWidth] = val;
        }

        // 三点中心差分梯度
        var grad = new double[total];
        grad[0]         = profile[1]         - profile[0];
        grad[total - 1] = profile[total - 1] - profile[total - 2];
        for (int i = 1; i < total - 1; i++)
            grad[i] = (profile[i + 1] - profile[i - 1]) / 2.0;

        // 找梯度绝对值最大处
        int    peakIdx = 0;
        double peakAbs = 0;
        for (int i = 0; i < total; i++)
        {
            double a = Math.Abs(grad[i]);
            if (a > peakAbs) { peakAbs = a; peakIdx = i; }
        }
        if (peakAbs < 1.0) return new SingleRoiDetection(null, 0);

        // 抛物线亚像素插值
        double sub = 0;
        if (peakIdx > 0 && peakIdx < total - 1)
        {
            double gm = Math.Abs(grad[peakIdx - 1]);
            double g0 = Math.Abs(grad[peakIdx]);
            double gp = Math.Abs(grad[peakIdx + 1]);
            double den = gm - 2 * g0 + gp;
            if (Math.Abs(den) > 1e-10)
                sub = Math.Clamp(0.5 * (gm - gp) / den, -1.0, 1.0);
        }

        double off = (peakIdx - halfWidth) + sub;
        return new SingleRoiDetection(
            new WpfPoint(center.X + off * px, center.Y + off * py),
            peakAbs);
    }

    private static double BilinearSample(Mat gray, double x, double y)
    {
        int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
        int x1 = x0 + 1,            y1 = y0 + 1;
        if (x0 < 0 || y0 < 0 || x1 >= gray.Cols || y1 >= gray.Rows)
            return double.NaN;
        double fx = x - x0, fy = y - y0;
        return gray.At<byte>(y0, x0) * (1 - fx) * (1 - fy)
             + gray.At<byte>(y0, x1) * fx       * (1 - fy)
             + gray.At<byte>(y1, x0) * (1 - fx) * fy
             + gray.At<byte>(y1, x1) * fx       * fy;
    }

    private static Mat ToGray(Mat src)
    {
        if (src.Channels() == 1) return src.Clone();
        var g = new Mat();
        Cv2.CvtColor(src, g, src.Channels() == 4
            ? ColorConversionCodes.BGRA2GRAY
            : ColorConversionCodes.BGR2GRAY);
        return g;
    }

    /// <summary>Gauss 消元求解 3×3 线性方程组 A·x = b</summary>
    private static bool SolveLinear3(double[,] A, double[] b,
        out double x0, out double x1, out double x2)
    {
        x0 = x1 = x2 = 0;
        // 增广矩阵 [A|b]（3×4）
        double[,] M = {
            { A[0,0], A[0,1], A[0,2], b[0] },
            { A[1,0], A[1,1], A[1,2], b[1] },
            { A[2,0], A[2,1], A[2,2], b[2] }
        };

        for (int col = 0; col < 3; col++)
        {
            // 选主元
            int pivot = col;
            for (int row = col + 1; row < 3; row++)
                if (Math.Abs(M[row, col]) > Math.Abs(M[pivot, col]))
                    pivot = row;

            if (Math.Abs(M[pivot, col]) < 1e-12) return false;

            // 交换行
            if (pivot != col)
                for (int k = 0; k <= 3; k++)
                    (M[col, k], M[pivot, k]) = (M[pivot, k], M[col, k]);

            // 消元
            for (int row = 0; row < 3; row++)
            {
                if (row == col) continue;
                double factor = M[row, col] / M[col, col];
                for (int k = col; k <= 3; k++)
                    M[row, k] -= factor * M[col, k];
            }
        }

        x0 = M[0, 3] / M[0, 0];
        x1 = M[1, 3] / M[1, 1];
        x2 = M[2, 3] / M[2, 2];
        return true;
    }
}
