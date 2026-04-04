namespace CaliperTool.Wpf.Models;

/// <summary>
/// 圆拟合结果（Kasa 代数最小二乘法输出）
/// </summary>
public sealed class CircleResult
{
    // ── 拟合结果 ──────────────────────────────────────────────
    /// <summary>拟合圆心 X（图像坐标，亚像素精度）</summary>
    public double CenterX { get; init; }

    /// <summary>拟合圆心 Y（图像坐标，亚像素精度）</summary>
    public double CenterY { get; init; }

    /// <summary>拟合半径（像素）</summary>
    public double Radius { get; init; }

    // ── 误差指标 ──────────────────────────────────────────────
    /// <summary>各有效边缘点到拟合圆的距离²均值（均方误差）</summary>
    public double MeanSquaredError { get; init; }

    /// <summary>均方根误差</summary>
    public double RootMeanSquaredError => Math.Sqrt(MeanSquaredError);

    /// <summary>所有有效点中到拟合圆距离的最大值</summary>
    public double MaxDeviation { get; init; }

    /// <summary>参与拟合的有效边缘点数量</summary>
    public int PointCount { get; init; }

    // ── 格式化显示 ─────────────────────────────────────────────
    public string CenterDisplay  => $"({CenterX:F4},  {CenterY:F4})";
    public string RadiusDisplay  => $"{Radius:F4} px";
    public string MseDisplay     => $"{MeanSquaredError:F4} px²";
    public string RmseDisplay    => $"{RootMeanSquaredError:F4} px";
    public string MaxDevDisplay  => $"{MaxDeviation:F4} px";
}
