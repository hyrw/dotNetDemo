using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ScaleAndTranslateTransform;

/// <summary>
/// 自定义 Matrix 动画
/// </summary>
public class MatrixAnimation : MatrixAnimationBase
{
    public static readonly DependencyProperty FromProperty =
        DependencyProperty.Register(
            nameof(From),
            typeof(Matrix?),
            typeof(MatrixAnimation));

    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register(
            nameof(To),
            typeof(Matrix?),
            typeof(MatrixAnimation));

    public Matrix? From
    {
        get { return (Matrix?)GetValue(FromProperty); }
        set { SetValue(FromProperty, value); }
    }

    public Matrix? To
    {
        get { return (Matrix?)GetValue(ToProperty); }
        set { SetValue(ToProperty, value); }
    }

    // 缓动函数
    public IEasingFunction? EasingFunction { get; set; }

    protected override Matrix GetCurrentValueCore(Matrix defaultOriginValue, Matrix defaultDestinationValue, AnimationClock animationClock)
    {
        if (!animationClock.CurrentProgress.HasValue)
            return defaultOriginValue;

        double progress = animationClock.CurrentProgress.Value;

        // 应用缓动函数
        if (EasingFunction != null)
        {
            progress = EasingFunction.Ease(progress);
        }

        // 起始矩阵
        Matrix fromMatrix = From ?? defaultOriginValue;
        // 目标矩阵
        Matrix toMatrix = To ?? defaultDestinationValue;

        // 插值计算矩阵
        return InterpolateMatrix(fromMatrix, toMatrix, progress);
    }

    /// <summary>
    /// 矩阵插值
    /// </summary>
    static Matrix InterpolateMatrix(Matrix from, Matrix to, double progress)
    {
        return new Matrix(
            from.M11 + (to.M11 - from.M11) * progress,
            from.M12 + (to.M12 - from.M12) * progress,
            from.M21 + (to.M21 - from.M21) * progress,
            from.M22 + (to.M22 - from.M22) * progress,
            from.OffsetX + (to.OffsetX - from.OffsetX) * progress,
            from.OffsetY + (to.OffsetY - from.OffsetY) * progress
        );
    }

    protected override Freezable CreateInstanceCore()
    {
        return new MatrixAnimation();
    }
}
