namespace CaliperTool.Wpf.Models;

/// <summary>
/// 直线拟合结果，包含方程形式、参数化形式和误差指标
/// </summary>
public sealed class LineResult
{
    // ── 参数化形式（Cv2.FitLine 直接输出）──────────────────
    // 直线上一点 (X0, Y0) + 方向向量 (Vx, Vy)
    // 满足：|(Vx, Vy)| = 1（单位方向向量）
    public double Vx { get; init; }
    public double Vy { get; init; }
    public double X0 { get; init; }
    public double Y0 { get; init; }

    // ── 直线方程形式 ────────────────────────────────────────
    // 情况1：非垂直线  y = Slope * x + Intercept
    // 情况2：垂直线    x = VerticalX
    public bool IsVertical { get; init; }
    public double Slope { get; init; }       // k（斜率）
    public double Intercept { get; init; }   // b（截距）
    public double VerticalX { get; init; }   // 垂直线的 x 坐标

    // ── 角度信息 ─────────────────────────────────────────────
    // 以 x 轴正方向为基准，角度范围 [0°, 180°)
    public double AngleDegrees { get; init; }

    // ── 误差指标 ─────────────────────────────────────────────
    /// <summary>均方误差（点到直线距离的平方均值）</summary>
    public double MeanSquaredError { get; init; }

    /// <summary>均方根误差（点到直线距离的均值）</summary>
    public double RootMeanSquaredError => Math.Sqrt(MeanSquaredError);

    /// <summary>R² 决定系数（越接近1拟合越好）</summary>
    public double RSquared { get; init; }

    /// <summary>参与拟合的点数</summary>
    public int PointCount { get; init; }

    // ── 格式化输出 ────────────────────────────────────────────

    /// <summary>格式化直线方程字符串</summary>
    public string LineEquationDisplay
    {
        get
        {
            if (IsVertical)
                return $"x = {VerticalX:F4}";

            string sign = Intercept >= 0 ? "+" : "-";
            return $"y = {Slope:F4}x {sign} {Math.Abs(Intercept):F4}";
        }
    }

    /// <summary>格式化参数化形式字符串（两行）</summary>
    public string ParametricFormDisplay =>
        $"(x, y) = ({X0:F4}, {Y0:F4}) + t·({Vx:F4}, {Vy:F4})";

    /// <summary>格式化误差指标字符串</summary>
    public string ErrorMetricsDisplay =>
        $"MSE: {MeanSquaredError:F4}\n" +
        $"RMSE: {RootMeanSquaredError:F4}\n" +
        $"R²: {RSquared:F6}";

    /// <summary>格式化角度字符串</summary>
    public string AngleDisplay => $"{AngleDegrees:F4}°";
}
