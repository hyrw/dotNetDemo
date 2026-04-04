using System.Windows;
using System.Windows.Input;
using CaliperTool.Wpf.ViewModels;

namespace CaliperTool.Wpf;

/// <summary>
/// MainWindow 后台：
///  - 持有两个独立 ViewModel（直线卡尺 / 圆卡尺）
///  - 通过工具栏 ToggleButton 切换当前活动模式
///  - 分别订阅两个 VM 的事件，路由到 TheImageCanvas 的绘图 API
///  - 右侧面板通过 Visibility 切换展示
/// </summary>
public partial class MainWindow : Window
{
    private readonly CaliperViewModel       _lineVm;
    private readonly CircleCaliperViewModel _circleVm;
    private bool _isLineMode = true;  // true = 直线卡尺，false = 圆卡尺

    public MainWindow()
    {
        InitializeComponent();

        _lineVm   = new CaliperViewModel();
        _circleVm = new CircleCaliperViewModel();

        // 同步 Slider 初始值到 VM
        _lineVm.RoiCount     = (int)SliderLineRoiCount.Value;
        _lineVm.RoiHalfWidth = (int)SliderLineRoiHalf.Value;
        _circleVm.RoiCount     = (int)SliderCircleRoiCount.Value;
        _circleVm.RoiHalfWidth = (int)SliderCircleRoiHalf.Value;
        _circleVm.StartAngle   = SliderStartAngle.Value;
        _circleVm.SweepAngle   = SliderSweepAngle.Value;

        // ── 直线卡尺 VM 事件订阅 ─────────────────────────────
        _lineVm.ImageLoadRequested += OnLineImageLoadRequested;
        _lineVm.AxisChanged        += OnLineAxisChanged;
        _lineVm.RoisChanged        += OnLineRoisChanged;
        _lineVm.DetectionCompleted += OnLineDetectionCompleted;
        _lineVm.AllReset           += OnLineAllReset;

        // ── 圆卡尺 VM 事件订阅 ───────────────────────────────
        _circleVm.ImageLoadRequested += OnCircleImageLoadRequested;
        _circleVm.AxisChanged        += OnCircleAxisChanged;
        _circleVm.RoisChanged        += OnCircleRoisChanged;
        _circleVm.DetectionCompleted += OnCircleDetectionCompleted;
        _circleVm.AllReset           += OnCircleAllReset;

        // ── ImageCanvas 点击事件路由到当前活动 VM ────────────
        TheImageCanvas.CanvasPointClicked += pt =>
        {
            if (_isLineMode) _lineVm.OnImagePointClicked(pt);
            else             _circleVm.OnImagePointClicked(pt);
        };

        // ── 键盘快捷键 ───────────────────────────────────────
        KeyDown += OnKeyDown;
    }

    // ── 模式切换 ──────────────────────────────────────────────
    private void OnLineModeClick(object sender, RoutedEventArgs e)
    {
        if (_isLineMode) { BtnLineMode.IsChecked = true; return; } // 已激活，不重复操作
        _isLineMode = true;
        BtnLineMode.IsChecked   = true;
        BtnCircleMode.IsChecked = false;
        PanelLine.Visibility            = Visibility.Visible;
        PanelCircleBorder.Visibility    = Visibility.Collapsed;
        TheImageCanvas.ClearAll();
        TxtStatus.Text = "切换到直线卡尺模式  |  请点击设置起点 P1";
    }

    private void OnCircleModeClick(object sender, RoutedEventArgs e)
    {
        if (!_isLineMode) { BtnCircleMode.IsChecked = true; return; }
        _isLineMode = false;
        BtnLineMode.IsChecked   = false;
        BtnCircleMode.IsChecked = true;
        PanelLine.Visibility         = Visibility.Collapsed;
        PanelCircleBorder.Visibility = Visibility.Visible;
        TheImageCanvas.ClearAll();
        TxtStatus.Text = "切换到圆卡尺模式  |  请点击设置第1个定义点（P1）";
    }

    // ── 工具栏按钮点击 ────────────────────────────────────────

    private void OnLoadImageClick(object sender, RoutedEventArgs e)
    {
        if (_isLineMode) _lineVm.LoadImageCommand.Execute(null);
        else             _circleVm.LoadImageCommand.Execute(null);
    }

    private void OnDetectClick(object sender, RoutedEventArgs e)
    {
        if (_isLineMode)
        {
            if (_lineVm.DetectCommand.CanExecute(null))
                _lineVm.DetectCommand.Execute(null);
        }
        else
        {
            if (_circleVm.DetectCommand.CanExecute(null))
                _circleVm.DetectCommand.Execute(null);
        }
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        if (_isLineMode)
        {
            if (_lineVm.ResetCommand.CanExecute(null))
                _lineVm.ResetCommand.Execute(null);
        }
        else
        {
            if (_circleVm.ResetCommand.CanExecute(null))
                _circleVm.ResetCommand.Execute(null);
        }
    }

    // ── Slider 变化处理 ───────────────────────────────────────

    private void OnLineRoiCountChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_lineVm is null) return;
        _lineVm.RoiCount = (int)e.NewValue;
        TxtLineRoiCount.Text = ((int)e.NewValue).ToString();
    }

    private void OnLineRoiHalfChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_lineVm is null) return;
        _lineVm.RoiHalfWidth = (int)e.NewValue;
        TxtLineRoiHalf.Text = ((int)e.NewValue).ToString();
    }

    private void OnCircleRoiCountChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_circleVm is null) return;
        _circleVm.RoiCount = (int)e.NewValue;
        TxtCircleRoiCount.Text = ((int)e.NewValue).ToString();
    }

    private void OnCircleRoiHalfChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_circleVm is null) return;
        _circleVm.RoiHalfWidth = (int)e.NewValue;
        TxtCircleRoiHalf.Text = ((int)e.NewValue).ToString();
    }

    private void OnStartAngleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_circleVm is null) return;
        _circleVm.StartAngle = e.NewValue;
        TxtStartAngle.Text = ((int)e.NewValue).ToString();
    }

    private void OnSweepAngleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_circleVm is null) return;
        _circleVm.SweepAngle = e.NewValue;
        TxtSweepAngle.Text = ((int)e.NewValue).ToString();
    }

    // ── 直线卡尺 VM 事件响应 ──────────────────────────────────

    private void OnLineImageLoadRequested(object? sender, string filePath)
    {
        var bmp = TheImageCanvas.LoadImageFromFile(filePath);
        if (bmp is not null)
        {
            _lineVm.OnImageLoaded(filePath);
            // 圆卡尺同步图像（共用同一个 ImageCanvas）
            _circleVm.OnImageLoaded(filePath);
            TxtStatus.Text = _lineVm.StatusMessage;
        }
        else
            MessageBox.Show("无法加载图像文件。", "加载失败",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnLineAxisChanged(object? sender, EventArgs e)
    {
        if (!_isLineMode) return;
        if (_lineVm.P1 is null) return;
        bool p1Only = _lineVm.P2 is null;
        TheImageCanvas.RefreshAxis(_lineVm.P1.Value, _lineVm.P2 ?? _lineVm.P1.Value, p1Only);
        if (!p1Only && _lineVm.CurrentCaliper is not null)
            TheImageCanvas.RefreshRois(_lineVm.CurrentCaliper);
        TxtStatus.Text = _lineVm.StatusMessage;
    }

    private void OnLineRoisChanged(object? sender, EventArgs e)
    {
        if (!_isLineMode || _lineVm.CurrentCaliper is null) return;
        TheImageCanvas.RefreshRois(_lineVm.CurrentCaliper);
        TheImageCanvas.ClearResults();
        TxtStatus.Text = _lineVm.StatusMessage;
    }

    private void OnLineDetectionCompleted(object? sender, EventArgs e)
    {
        if (!_isLineMode) return;
        using var mat = TheImageCanvas.ImageService.GetCurrentMat();
        if (mat is null || mat.Empty()) return;
        _lineVm.RunDetection(mat);
        if (_lineVm.RoiResults is not null)
            TheImageCanvas.RefreshEdgePoints(_lineVm.RoiResults);
        TheImageCanvas.RefreshFitLine(_lineVm.FittingResult);
        UpdateLineResultPanel();
        TxtStatus.Text = _lineVm.StatusMessage;
    }

    private void OnLineAllReset(object? sender, EventArgs e)
    {
        if (!_isLineMode) return;
        TheImageCanvas.ClearAll();
        ResetLineResultPanel();
        TxtStatus.Text = _lineVm.StatusMessage;
    }

    // ── 圆卡尺 VM 事件响应 ────────────────────────────────────

    private void OnCircleImageLoadRequested(object? sender, string filePath)
    {
        var bmp = TheImageCanvas.LoadImageFromFile(filePath);
        if (bmp is not null)
        {
            _circleVm.OnImageLoaded(filePath);
            _lineVm.OnImageLoaded(filePath);
            TxtStatus.Text = _circleVm.StatusMessage;
        }
        else
            MessageBox.Show("无法加载图像文件。", "加载失败",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnCircleAxisChanged(object? sender, EventArgs e)
    {
        if (_isLineMode) return;
        bool hasPreview = _circleVm.P3 is not null && _circleVm.RefR > 0;
        TheImageCanvas.RefreshCircleAxis(
            _circleVm.P1, _circleVm.P2, _circleVm.P3,
            _circleVm.RefCx, _circleVm.RefCy, _circleVm.RefR,
            hasPreview);
        if (hasPreview)
            TheImageCanvas.RefreshCircleRois(
                _circleVm.RefCx, _circleVm.RefCy, _circleVm.RefR,
                _circleVm.RoiCount, _circleVm.RoiHalfWidth,
                _circleVm.StartAngle, _circleVm.SweepAngle);
        TxtStatus.Text = _circleVm.StatusMessage;
    }

    private void OnCircleRoisChanged(object? sender, EventArgs e)
    {
        if (_isLineMode || _circleVm.RefR <= 0) return;
        TheImageCanvas.RefreshCircleRois(
            _circleVm.RefCx, _circleVm.RefCy, _circleVm.RefR,
            _circleVm.RoiCount, _circleVm.RoiHalfWidth,
            _circleVm.StartAngle, _circleVm.SweepAngle);
        TheImageCanvas.ClearResults();
        TxtStatus.Text = _circleVm.StatusMessage;
    }

    private void OnCircleDetectionCompleted(object? sender, EventArgs e)
    {
        if (_isLineMode) return;
        using var mat = TheImageCanvas.ImageService.GetCurrentMat();
        if (mat is null || mat.Empty()) return;
        _circleVm.RunDetection(mat);
        if (_circleVm.RoiResults is not null)
            TheImageCanvas.RefreshEdgePoints(_circleVm.RoiResults);
        TheImageCanvas.RefreshFittedCircle(_circleVm.CircleResult);
        UpdateCircleResultPanel();
        TxtStatus.Text = _circleVm.StatusMessage;
    }

    private void OnCircleAllReset(object? sender, EventArgs e)
    {
        if (_isLineMode) return;
        TheImageCanvas.ClearAll();
        ResetCircleResultPanel();
        TxtStatus.Text = _circleVm.StatusMessage;
    }

    // ── 面板结果刷新 ──────────────────────────────────────────

    private void UpdateLineResultPanel()
    {
        TxtLineValidPts.Text = _lineVm.ValidPointsDisplay;
        TxtLineEq.Text       = _lineVm.LineEquationDisplay;
        TxtLineParam.Text    = _lineVm.ParametricFormDisplay;
        TxtLineAngle.Text    = _lineVm.AngleDisplay;
        TxtLineMse.Text      = _lineVm.MseDisplay;
        TxtLineRmse.Text     = _lineVm.RmseDisplay;
        TxtLineR2.Text       = _lineVm.RSquaredDisplay;
    }

    private void ResetLineResultPanel()
    {
        TxtLineValidPts.Text = TxtLineEq.Text = TxtLineParam.Text =
        TxtLineAngle.Text = TxtLineMse.Text = TxtLineRmse.Text =
        TxtLineR2.Text = "—";
    }

    private void UpdateCircleResultPanel()
    {
        TxtCircleValidPts.Text = _circleVm.ValidPointsDisplay;
        TxtCircleCenter.Text   = _circleVm.CenterDisplay;
        TxtCircleRadius.Text   = _circleVm.RadiusDisplay;
        TxtCircleMse.Text      = _circleVm.MseDisplay;
        TxtCircleRmse.Text     = _circleVm.RmseDisplay;
        TxtCircleMaxDev.Text   = _circleVm.MaxDevDisplay;
    }

    private void ResetCircleResultPanel()
    {
        TxtCircleValidPts.Text = TxtCircleCenter.Text = TxtCircleRadius.Text =
        TxtCircleMse.Text = TxtCircleRmse.Text = TxtCircleMaxDev.Text = "—";
    }

    // ── 键盘快捷键 ────────────────────────────────────────────

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        bool ctrl = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);
        switch (e.Key)
        {
            case Key.O when ctrl:
                OnLoadImageClick(this, e);
                e.Handled = true; break;
            case Key.F5:
                OnDetectClick(this, e);
                e.Handled = true; break;
            case Key.Escape:
                OnResetClick(this, e);
                e.Handled = true; break;
        }
    }
}
