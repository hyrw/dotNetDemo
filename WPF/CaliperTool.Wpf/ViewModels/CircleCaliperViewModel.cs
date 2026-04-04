using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CaliperTool.Wpf.Models;
using CaliperTool.Wpf.Services;
using Microsoft.Win32;

namespace CaliperTool.Wpf.ViewModels;

// ── 圆卡尺状态机 ──────────────────────────────────────────────
public enum CircleCaliperState
{
    NoImage,      // 未加载图像
    WaitingP1,    // 等待第1个定义点
    WaitingP2,    // 已设 P1，等待第2点
    WaitingP3,    // 已设 P1/P2，等待第3点
    Ready,        // 三点已定，卡尺就绪，可执行检测
    Detected      // 检测完成，显示结果
}

/// <summary>
/// 圆卡尺工具 ViewModel（独立状态机，与 CaliperViewModel 并行）
/// </summary>
public sealed partial class CircleCaliperViewModel : ObservableObject
{
    // ── 服务 ──────────────────────────────────────────────────
    private readonly CircleCaliperService _service = new();

    // ── 状态 ──────────────────────────────────────────────────
    [ObservableProperty] private CircleCaliperState _state = CircleCaliperState.NoImage;
    [ObservableProperty] private string _statusMessage = "请先加载图像";
    [ObservableProperty] private bool _hasImage;

    // ── 三个定义点（图像坐标）───────────────────────────────
    private Point? _p1, _p2, _p3;

    // 由三点解析的参考圆（用于绘制预览）
    private double _refCx, _refCy, _refR;
    public double RefCx => _refCx;
    public double RefCy => _refCy;
    public double RefR  => _refR;

    // ── ROI 参数 ──────────────────────────────────────────────
    private int _roiCount = 16;
    public int RoiCount
    {
        get => _roiCount;
        set { if (SetProperty(ref _roiCount, value)) OnRoiParamChanged(); }
    }

    private int _roiHalfWidth = 20;
    public int RoiHalfWidth
    {
        get => _roiHalfWidth;
        set { if (SetProperty(ref _roiHalfWidth, value)) OnRoiParamChanged(); }
    }

    private double _startAngle = 0;
    public double StartAngle
    {
        get => _startAngle;
        set { if (SetProperty(ref _startAngle, value)) OnRoiParamChanged(); }
    }

    private double _sweepAngle = 360;
    public double SweepAngle
    {
        get => _sweepAngle;
        set { if (SetProperty(ref _sweepAngle, Math.Max(10, value))) OnRoiParamChanged(); }
    }

    // ── 检测结果 ──────────────────────────────────────────────
    [ObservableProperty] private IReadOnlyList<RoiResult>? _roiResults;
    [ObservableProperty] private CircleResult? _circleResult;

    // ── 结果显示属性 ──────────────────────────────────────────
    [ObservableProperty] private string _centerDisplay      = "—";
    [ObservableProperty] private string _radiusDisplay      = "—";
    [ObservableProperty] private string _validPointsDisplay = "—";
    [ObservableProperty] private string _mseDisplay         = "—";
    [ObservableProperty] private string _rmseDisplay        = "—";
    [ObservableProperty] private string _maxDevDisplay      = "—";

    // ── 对外事件 ──────────────────────────────────────────────
    public event EventHandler<string>? ImageLoadRequested;
    public event EventHandler?         AxisChanged;         // 点或参数变化 → 刷新轴线+ROI
    public event EventHandler?         RoisChanged;         // ROI 参数变化 → 仅刷新 ROI
    public event EventHandler?         DetectionCompleted;  // 检测完成
    public event EventHandler?         AllReset;            // 全部重置

    // ── 命令 CanExecute ───────────────────────────────────────
    private bool CanDetect => State == CircleCaliperState.Ready
                           || State == CircleCaliperState.Detected;
    private bool CanReset  => State != CircleCaliperState.NoImage
                           && State != CircleCaliperState.WaitingP1;

    // ── 命令 ─────────────────────────────────────────────────

    [RelayCommand]
    private void LoadImage()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "选择图像文件",
            Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|所有文件|*.*"
        };
        if (dlg.ShowDialog() != true) return;
        ImageLoadRequested?.Invoke(this, dlg.FileName);
    }

    public void OnImageLoaded(string filePath)
    {
        HasImage = true;
        ResetAll();
        State         = CircleCaliperState.WaitingP1;
        StatusMessage = "请在图像上点击第1个点（P1）来定义参考圆";
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanDetect))]
    private void Detect() => DetectionCompleted?.Invoke(this, EventArgs.Empty);

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        ResetAll();
        State         = CircleCaliperState.WaitingP1;
        StatusMessage = "已重置，请点击设置 P1";
        AllReset?.Invoke(this, EventArgs.Empty);
        NotifyCommands();
    }

    // ── 点击处理 ──────────────────────────────────────────────

    public void OnImagePointClicked(Point imagePoint)
    {
        switch (State)
        {
            case CircleCaliperState.WaitingP1:
                _p1           = imagePoint;
                State         = CircleCaliperState.WaitingP2;
                StatusMessage = $"P1 = ({imagePoint.X:F1}, {imagePoint.Y:F1})  请点击第2个点（P2）";
                AxisChanged?.Invoke(this, EventArgs.Empty);
                NotifyCommands();
                break;

            case CircleCaliperState.WaitingP2:
                _p2           = imagePoint;
                State         = CircleCaliperState.WaitingP3;
                StatusMessage = $"P2 = ({imagePoint.X:F1}, {imagePoint.Y:F1})  请点击第3个点（P3）";
                AxisChanged?.Invoke(this, EventArgs.Empty);
                NotifyCommands();
                break;

            case CircleCaliperState.WaitingP3:
                _p3 = imagePoint;
                var circle = CircleCaliperService.ThreePointsToCircle(
                    _p1!.Value, _p2!.Value, _p3!.Value);

                if (circle is null)
                {
                    StatusMessage = "三点共线，无法定义圆，请重新点击 P3";
                    _p3 = null;
                    return;
                }

                (_refCx, _refCy, _refR) = circle.Value;
                State = CircleCaliperState.Ready;
                StatusMessage = $"P3 = ({imagePoint.X:F1}, {imagePoint.Y:F1})  |  " +
                                $"参考圆: 圆心({_refCx:F1},{_refCy:F1}) R={_refR:F1}  |  请执行检测";
                AxisChanged?.Invoke(this, EventArgs.Empty);
                RoisChanged?.Invoke(this, EventArgs.Empty);
                NotifyCommands();
                break;
        }
    }

    /// <summary>
    /// 执行边缘检测（由 View 在 DetectionCompleted 事件中调用）
    /// </summary>
    public void RunDetection(OpenCvSharp.Mat image)
    {
        if (image.Empty() || _refR <= 0) return;

        try
        {
            var results = _service.DetectEdgePoints(
                image, _refCx, _refCy, _refR,
                _roiCount, _roiHalfWidth,
                _startAngle, _sweepAngle);

            RoiResults = results;

            var fit = CircleCaliperService.FitCircle(results);
            CircleResult = fit;

            if (fit is not null)
            {
                UpdateDisplayProperties(fit, results);
                StatusMessage = $"检测完成  |  有效点 {fit.PointCount}/{results.Count}  |  " +
                                $"圆心({fit.CenterX:F2},{fit.CenterY:F2})  R={fit.Radius:F2}";
            }
            else
            {
                ResetDisplayProperties();
                int valid = results.Count(r => r.IsValid);
                StatusMessage = valid < 3
                    ? $"有效边缘点不足（{valid} 个，至少需要3个），请调整卡尺或 ROI 参数"
                    : "圆拟合失败，请检查边缘点分布";
            }

            State = CircleCaliperState.Detected;
            NotifyCommands();
        }
        catch (Exception ex)
        {
            StatusMessage = $"检测出错：{ex.Message}";
        }
    }

    // ── 对外只读属性（View 绘图用）────────────────────────────
    public Point? P1 => _p1;
    public Point? P2 => _p2;
    public Point? P3 => _p3;

    // ── 私有辅助 ──────────────────────────────────────────────

    private void OnRoiParamChanged()
    {
        if (State is not (CircleCaliperState.Ready or CircleCaliperState.Detected)) return;
        if (State == CircleCaliperState.Detected)
        {
            State        = CircleCaliperState.Ready;
            RoiResults   = null;
            CircleResult = null;
            ResetDisplayProperties();
        }
        RoisChanged?.Invoke(this, EventArgs.Empty);
        StatusMessage = $"ROI 已更新（{_roiCount} 个 × 半宽 {_roiHalfWidth}px）  |  请重新执行检测";
        NotifyCommands();
    }

    private void ResetAll()
    {
        _p1 = _p2 = _p3 = null;
        _refCx = _refCy = _refR = 0;
        RoiResults   = null;
        CircleResult = null;
        ResetDisplayProperties();
    }

    private void UpdateDisplayProperties(CircleResult r, IReadOnlyList<RoiResult> all)
    {
        CenterDisplay      = r.CenterDisplay;
        RadiusDisplay      = r.RadiusDisplay;
        ValidPointsDisplay = $"{r.PointCount} / {all.Count}";
        MseDisplay         = r.MseDisplay;
        RmseDisplay        = r.RmseDisplay;
        MaxDevDisplay      = r.MaxDevDisplay;
    }

    private void ResetDisplayProperties()
    {
        CenterDisplay = RadiusDisplay = ValidPointsDisplay =
        MseDisplay = RmseDisplay = MaxDevDisplay = "—";
    }

    private void NotifyCommands()
    {
        DetectCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
    }
}
