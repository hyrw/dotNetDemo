using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CaliperTool.Wpf.Models;
using CaliperTool.Wpf.Services;
using Microsoft.Win32;

namespace CaliperTool.Wpf.ViewModels;

// ── 卡尺工具状态机 ────────────────────────────────────────────
public enum CaliperState
{
    NoImage,      // 未加载图像
    WaitingP1,    // 等待点击第1个端点
    WaitingP2,    // 已设置 P1，等待第2个端点
    Ready,        // 卡尺就绪（可执行检测）
    Detected      // 已完成检测（显示结果）
}

/// <summary>
/// 直线卡尺工具主 ViewModel
/// 状态机：NoImage → WaitingP1 → WaitingP2 → Ready → Detected
/// </summary>
public sealed partial class CaliperViewModel : ObservableObject
{
    // ── 服务 ──────────────────────────────────────────────────
    private readonly CaliperService _caliperService = new();

    // ── 状态 ──────────────────────────────────────────────────
    [ObservableProperty] private CaliperState _state = CaliperState.NoImage;
    [ObservableProperty] private string _statusMessage = "请先加载图像";
    [ObservableProperty] private bool _hasImage;

    // ── 卡尺端点（图像坐标）─────────────────────────────────
    private Point? _p1;
    private Point? _p2;

    // ── ROI 参数（Slider 绑定）──────────────────────────────

    private int _roiCount = 10;
    public int RoiCount
    {
        get => _roiCount;
        set
        {
            if (SetProperty(ref _roiCount, value))
                OnRoiParamChanged();
        }
    }

    private int _roiHalfWidth = 20;
    public int RoiHalfWidth
    {
        get => _roiHalfWidth;
        set
        {
            if (SetProperty(ref _roiHalfWidth, value))
                OnRoiParamChanged();
        }
    }

    // ── 当前卡尺定义 ──────────────────────────────────────────
    public CaliperLine? CurrentCaliper { get; private set; }

    // ── 检测结果 ──────────────────────────────────────────────
    [ObservableProperty] private IReadOnlyList<RoiResult>? _roiResults;
    [ObservableProperty] private LineResult? _fittingResult;

    // ── 结果显示属性 ──────────────────────────────────────────
    [ObservableProperty] private string _lineEquationDisplay   = "—";
    [ObservableProperty] private string _parametricFormDisplay = "—";
    [ObservableProperty] private string _angleDisplay          = "—";
    [ObservableProperty] private string _mseDisplay            = "—";
    [ObservableProperty] private string _rmseDisplay           = "—";
    [ObservableProperty] private string _rSquaredDisplay       = "—";
    [ObservableProperty] private string _validPointsDisplay    = "—";

    // ── 对外事件（View 订阅后执行对应绘图刷新）───────────────
    public event EventHandler<string>?  ImageLoadRequested;
    public event EventHandler?          AxisChanged;        // P1/P2 变化
    public event EventHandler?          RoisChanged;        // ROI 参数变化
    public event EventHandler?          DetectionCompleted; // 检测完成
    public event EventHandler?          AllReset;           // 全部重置

    // ── 命令 CanExecute ───────────────────────────────────────
    private bool CanDetect  => State == CaliperState.Ready || State == CaliperState.Detected;
    private bool CanReset   => State != CaliperState.NoImage && State != CaliperState.WaitingP1;

    // ── 命令实现 ──────────────────────────────────────────────

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

    /// <summary>图像加载成功后由 View 回调</summary>
    public void OnImageLoaded(string filePath)
    {
        HasImage = true;
        ResetCaliper();
        State         = CaliperState.WaitingP1;
        StatusMessage = "请在图像上点击设置卡尺起点 P1";
        NotifyCommands();
    }

    [RelayCommand(CanExecute = nameof(CanDetect))]
    private void Detect()
    {
        // DetectCommand 通知 View 执行检测（View 提供 Mat）
        // 实际检测在 RunDetection 中由 View 传入 Mat 后执行
        DetectionCompleted?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        ResetCaliper();
        State         = CaliperState.WaitingP1;
        StatusMessage = "已重置，请点击设置卡尺起点 P1";
        AllReset?.Invoke(this, EventArgs.Empty);
        NotifyCommands();
    }

    // ── 点击处理（由 View 调用）──────────────────────────────

    /// <summary>用户在图像上点击，根据当前状态决定处理逻辑</summary>
    public void OnImagePointClicked(Point imagePoint)
    {
        switch (State)
        {
            case CaliperState.WaitingP1:
                _p1           = imagePoint;
                State         = CaliperState.WaitingP2;
                StatusMessage = $"P1 = ({imagePoint.X:F1}, {imagePoint.Y:F1})  请点击设置终点 P2";
                AxisChanged?.Invoke(this, EventArgs.Empty);
                NotifyCommands();
                break;

            case CaliperState.WaitingP2:
                _p2 = imagePoint;
                BuildCaliper();
                State         = CaliperState.Ready;
                StatusMessage = $"卡尺就绪  |  P2 = ({imagePoint.X:F1}, {imagePoint.Y:F1})  |  调整参数后点击「执行检测」";
                AxisChanged?.Invoke(this, EventArgs.Empty);
                RoisChanged?.Invoke(this, EventArgs.Empty);
                NotifyCommands();
                break;
        }
    }

    /// <summary>
    /// 执行边缘检测（由 View 在 DetectionCompleted 事件中调用，传入 OpenCV Mat）
    /// </summary>
    public void RunDetection(OpenCvSharp.Mat image)
    {
        if (CurrentCaliper is null || image.Empty()) return;

        try
        {
            var results = _caliperService.DetectEdgePoints(image, CurrentCaliper);
            RoiResults  = results;

            var fit     = _caliperService.FitLine(results);
            FittingResult = fit;

            if (fit is not null)
            {
                UpdateDisplayProperties(fit, results);
                StatusMessage = $"检测完成  |  有效点 {fit.PointCount}/{results.Count}  |  R² = {fit.RSquared:F4}";
            }
            else
            {
                ResetDisplayProperties();
                int valid = results.Count(r => r.IsValid);
                StatusMessage = valid < 2
                    ? $"有效边缘点不足（{valid} 个），请调整卡尺位置或 ROI 参数"
                    : "拟合失败，请检查图像质量";
            }

            State = CaliperState.Detected;
            NotifyCommands();
        }
        catch (Exception ex)
        {
            StatusMessage = $"检测出错：{ex.Message}";
        }
    }

    // ── 私有辅助 ──────────────────────────────────────────────

    private void BuildCaliper()
    {
        if (_p1.HasValue && _p2.HasValue)
            CurrentCaliper = new CaliperLine(_p1.Value, _p2.Value, _roiCount, _roiHalfWidth);
    }

    private void OnRoiParamChanged()
    {
        if (State is not (CaliperState.Ready or CaliperState.Detected)) return;
        BuildCaliper();
        // 参数变化时清除旧检测结果，重新显示 ROI
        if (State == CaliperState.Detected)
        {
            State           = CaliperState.Ready;
            RoiResults      = null;
            FittingResult   = null;
            ResetDisplayProperties();
        }
        RoisChanged?.Invoke(this, EventArgs.Empty);
        StatusMessage = $"ROI 已更新（{_roiCount} 个 × 半宽 {_roiHalfWidth}px）  |  请重新执行检测";
        NotifyCommands();
    }

    private void ResetCaliper()
    {
        _p1 = _p2 = null;
        CurrentCaliper = null;
        RoiResults     = null;
        FittingResult  = null;
        ResetDisplayProperties();
    }

    private void UpdateDisplayProperties(LineResult r, IReadOnlyList<RoiResult> all)
    {
        LineEquationDisplay   = r.LineEquationDisplay;
        ParametricFormDisplay = r.ParametricFormDisplay;
        AngleDisplay          = r.AngleDisplay;
        MseDisplay            = $"{r.MeanSquaredError:F4} px²";
        RmseDisplay           = $"{r.RootMeanSquaredError:F4} px";
        RSquaredDisplay       = $"{r.RSquared:F6}";
        ValidPointsDisplay    = $"{r.PointCount} / {all.Count}";
    }

    private void ResetDisplayProperties()
    {
        LineEquationDisplay = ParametricFormDisplay = AngleDisplay =
        MseDisplay = RmseDisplay = RSquaredDisplay = ValidPointsDisplay = "—";
    }

    private void NotifyCommands()
    {
        DetectCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
    }

    // ── P1 / P2 只读访问（供 View 绘图使用）─────────────────
    public Point? P1 => _p1;
    public Point? P2 => _p2;
}
