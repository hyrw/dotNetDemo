using WpfPoint = System.Windows.Point;

namespace CaliperTool.Wpf.Models;

/// <summary>
/// 单个 ROI 的边缘检测结果
/// </summary>
public sealed class RoiResult
{
    /// <summary>ROI 序号（0-based）</summary>
    public int Index { get; }

    /// <summary>ROI 中心点（图像坐标）</summary>
    public WpfPoint Center { get; }

    /// <summary>检测到的边缘点（图像坐标，亚像素精度）；未检测到时为 null</summary>
    public WpfPoint? EdgePoint { get; }

    /// <summary>沿搜索方向的最大梯度绝对值（反映边缘强度）</summary>
    public double MaxGradient { get; }

    /// <summary>是否成功检测到有效边缘点</summary>
    public bool IsValid => EdgePoint.HasValue && MaxGradient > 0;

    public RoiResult(int index, WpfPoint center, WpfPoint? edgePoint, double maxGradient)
    {
        Index       = index;
        Center      = center;
        EdgePoint   = edgePoint;
        MaxGradient = maxGradient;
    }

    /// <summary>创建一个无效结果（未检测到边缘）</summary>
    public static RoiResult Invalid(int index, WpfPoint center) =>
        new(index, center, null, 0);
}
