using CaliperTool.Wpf.Models;
using OpenCvSharp;
using WpfPoint = System.Windows.Point;

namespace CaliperTool.Wpf.Services;

/// <summary>
/// 直线卡尺核心服务：
///   1. 对每个 ROI 沿垂直方向提取灰度剖面，计算一阶梯度，亚像素定位边缘点
///   2. 对所有有效边缘点执行 Cv2.FitLine 直线拟合
/// </summary>
public sealed class CaliperService
{
    // ── 公共 API ──────────────────────────────────────────────

    /// <summary>
    /// 对卡尺各 ROI 进行边缘检测，返回每个 ROI 的检测结果
    /// </summary>
    /// <param name="image">源图像（BGR 或灰度 Mat）</param>
    /// <param name="caliper">卡尺定义</param>
    public IReadOnlyList<RoiResult> DetectEdgePoints(Mat image, CaliperLine caliper)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(caliper);
        if (image.Empty()) throw new ArgumentException("图像为空。");

        // 统一转为灰度图
        using var gray = ToGray(image);

        var centers = caliper.GetRoiCenters();
        var results = new List<RoiResult>(centers.Count);

        var (px, py) = caliper.PerpDirection; // 垂直方向单位向量

        for (int i = 0; i < centers.Count; i++)
        {
            var center = centers[i];
            var roi    = DetectSingleRoi(gray, center, px, py, caliper.RoiHalfWidth);
            results.Add(new RoiResult(i, center, roi.EdgePoint, roi.MaxGradient));
        }

        return results;
    }

    /// <summary>
    /// 对有效边缘点执行 Cv2.FitLine 直线拟合
    /// </summary>
    /// <returns>拟合结果；有效点不足2个时返回 null</returns>
    public LineResult? FitLine(IReadOnlyList<RoiResult> roiResults)
    {
        var validPts = roiResults
            .Where(r => r.IsValid)
            .Select(r => r.EdgePoint!.Value)
            .ToList();

        if (validPts.Count < 2) return null;

        // 转为 OpenCV Point2f
        var cvPts = validPts
            .Select(p => new Point2f((float)p.X, (float)p.Y))
            .ToArray();

        // Cv2.FitLine 输出 Mat（1×4 float）：[vx, vy, x0, y0]
        using var lineParamsMat = new Mat();
        Cv2.FitLine(
            InputArray.Create(cvPts),
            lineParamsMat,
            DistanceTypes.L2,
            param: 0,
            reps: 0.01,
            aeps: 0.01);

        double vx = lineParamsMat.At<float>(0);
        double vy = lineParamsMat.At<float>(1);
        double x0 = lineParamsMat.At<float>(2);
        double y0 = lineParamsMat.At<float>(3);

        // ── 转换为直线方程形式 ──────────────────────────────
        const double vertThr = 1e-6;
        bool isVertical = Math.Abs(vx) < vertThr;

        double slope     = isVertical ? 0 : vy / vx;
        double intercept = isVertical ? 0 : y0 - slope * x0;
        double verticalX = x0;

        double angleRad = Math.Atan2(vy, vx);
        double angleDeg = angleRad * 180.0 / Math.PI;
        if (angleDeg < 0) angleDeg += 180.0;

        double mse      = CalcMse(validPts, vx, vy, x0, y0);
        double rSquared = CalcRSquared(validPts, slope, intercept, isVertical, verticalX);

        return new LineResult
        {
            Vx = vx, Vy = vy, X0 = x0, Y0 = y0,
            IsVertical  = isVertical,
            Slope       = slope,
            Intercept   = intercept,
            VerticalX   = verticalX,
            AngleDegrees = angleDeg,
            MeanSquaredError = mse,
            RSquared    = rSquared,
            PointCount  = validPts.Count
        };
    }

    // ── 单个 ROI 的边缘检测 ───────────────────────────────────

    private record struct SingleRoiDetection(WpfPoint? EdgePoint, double MaxGradient);

    /// <summary>
    /// 在灰度图中，沿 (px, py) 方向从 center 出发提取像素剖面，
    /// 计算一阶梯度，用抛物线插值求亚像素边缘位置。
    /// </summary>
    private static SingleRoiDetection DetectSingleRoi(
        Mat gray,
        WpfPoint center,
        double px, double py,   // 垂直方向单位向量
        int halfWidth)
    {
        int total = halfWidth * 2 + 1; // 采样点总数（含中心）

        // ── 提取灰度剖面 ─────────────────────────────────────
        var profile = new double[total];
        int validCount = 0;

        for (int k = -halfWidth; k <= halfWidth; k++)
        {
            double sx = center.X + k * px;
            double sy = center.Y + k * py;

            // 双线性插值获取亚像素灰度值
            double val = BilinearSample(gray, sx, sy);
            if (double.IsNaN(val))
            {
                // 超出图像边界，标记整个 ROI 无效
                return new SingleRoiDetection(null, 0);
            }
            profile[k + halfWidth] = val;
            validCount++;
        }

        if (validCount < 3) return new SingleRoiDetection(null, 0);

        // ── 计算一阶梯度（Sobel-1D，三点差分）───────────────
        var gradient = new double[total];
        for (int i = 1; i < total - 1; i++)
            gradient[i] = (profile[i + 1] - profile[i - 1]) / 2.0;
        // 端点用单侧差分
        gradient[0]         = profile[1]          - profile[0];
        gradient[total - 1] = profile[total - 1]  - profile[total - 2];

        // ── 找梯度绝对值最大处 ───────────────────────────────
        int peakIdx  = 0;
        double peakAbs = 0;
        for (int i = 0; i < total; i++)
        {
            double a = Math.Abs(gradient[i]);
            if (a > peakAbs) { peakAbs = a; peakIdx = i; }
        }

        if (peakAbs < 1.0) return new SingleRoiDetection(null, 0); // 梯度太小，无明显边缘

        // ── 抛物线亚像素插值 ──────────────────────────────────
        double subPixelOffset = 0;
        if (peakIdx > 0 && peakIdx < total - 1)
        {
            double gm1 = Math.Abs(gradient[peakIdx - 1]);
            double g0  = Math.Abs(gradient[peakIdx]);
            double gp1 = Math.Abs(gradient[peakIdx + 1]);
            double denom = gm1 - 2 * g0 + gp1;
            if (Math.Abs(denom) > 1e-10)
                subPixelOffset = 0.5 * (gm1 - gp1) / denom;
            subPixelOffset = Math.Clamp(subPixelOffset, -1.0, 1.0);
        }

        // 将剖面索引（相对 center）转回图像坐标
        double edgeOffset = (peakIdx - halfWidth) + subPixelOffset;
        double edgeX = center.X + edgeOffset * px;
        double edgeY = center.Y + edgeOffset * py;

        return new SingleRoiDetection(new WpfPoint(edgeX, edgeY), peakAbs);
    }

    // ── 工具方法 ──────────────────────────────────────────────

    /// <summary>双线性插值采样（返回 NaN 表示越界）</summary>
    private static double BilinearSample(Mat gray, double x, double y)
    {
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        if (x0 < 0 || y0 < 0 || x1 >= gray.Cols || y1 >= gray.Rows)
            return double.NaN;

        double fx = x - x0;
        double fy = y - y0;

        double v00 = gray.At<byte>(y0, x0);
        double v10 = gray.At<byte>(y0, x1);
        double v01 = gray.At<byte>(y1, x0);
        double v11 = gray.At<byte>(y1, x1);

        return v00 * (1 - fx) * (1 - fy)
             + v10 * fx       * (1 - fy)
             + v01 * (1 - fx) * fy
             + v11 * fx       * fy;
    }

    private static Mat ToGray(Mat src)
    {
        if (src.Channels() == 1) return src.Clone();
        var gray = new Mat();
        Cv2.CvtColor(src, gray, src.Channels() == 4
            ? ColorConversionCodes.BGRA2GRAY
            : ColorConversionCodes.BGR2GRAY);
        return gray;
    }

    // ── 误差计算（与原 LineFittingService 相同）──────────────

    private static double CalcMse(
        List<WpfPoint> pts, double vx, double vy, double x0, double y0)
    {
        double sum = pts.Sum(p =>
        {
            double d = (p.X - x0) * vy - (p.Y - y0) * vx;
            return d * d;
        });
        return sum / pts.Count;
    }

    private static double CalcRSquared(
        List<WpfPoint> pts, double slope, double intercept,
        bool isVertical, double verticalX)
    {
        if (isVertical)
        {
            double mx   = pts.Average(p => p.X);
            double ssRes = pts.Sum(p => Math.Pow(p.X - verticalX, 2));
            double ssTot = pts.Sum(p => Math.Pow(p.X - mx, 2));
            return ssTot < 1e-12 ? 1.0 : 1.0 - ssRes / ssTot;
        }
        else
        {
            double my    = pts.Average(p => p.Y);
            double ssRes = pts.Sum(p => Math.Pow(p.Y - (slope * p.X + intercept), 2));
            double ssTot = pts.Sum(p => Math.Pow(p.Y - my, 2));
            return ssTot < 1e-12 ? 1.0 : 1.0 - ssRes / ssTot;
        }
    }
}
