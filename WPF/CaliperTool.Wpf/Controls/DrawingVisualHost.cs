using System.Windows;
using System.Windows.Media;

namespace CaliperTool.Wpf.Controls;

/// <summary>
/// DrawingVisual 宿主控件：管理4层 ContainerVisual
///   Layer 0 – 卡尺轴线 + 端点（蓝色）
///   Layer 1 – ROI 矩形框（橙色）
///   Layer 2 – 边缘检测点（红色）
///   Layer 3 – 拟合直线（绿色）
/// </summary>
public sealed class DrawingVisualHost : FrameworkElement
{
    private readonly ContainerVisual _root       = new();
    private readonly ContainerVisual _axisLayer  = new(); // 轴线+端点
    private readonly ContainerVisual _roiLayer   = new(); // ROI 矩形
    private readonly ContainerVisual _edgeLayer  = new(); // 边缘点
    private readonly ContainerVisual _lineLayer  = new(); // 拟合直线

    public DrawingVisualHost()
    {
        _root.Children.Add(_axisLayer);
        _root.Children.Add(_roiLayer);
        _root.Children.Add(_edgeLayer);
        _root.Children.Add(_lineLayer);
        AddVisualChild(_root);
        AddLogicalChild(_root);
    }

    // ── FrameworkElement 协议 ─────────────────────────────────
    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
        return _root;
    }

    /// <summary>绘制透明背景，使整个区域可接收 HitTest（鼠标事件）</summary>
    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null,
            new Rect(0, 0, ActualWidth, ActualHeight));
    }

    // ── 各层操作 API ──────────────────────────────────────────

    /// <summary>设置卡尺轴线层内容（蓝色端点 + 轴线）</summary>
    public void SetAxisVisual(DrawingVisual? v) => SetLayer(_axisLayer, v);

    /// <summary>设置 ROI 矩形层内容（橙色矩形框）</summary>
    public void SetRoiVisual(DrawingVisual? v) => SetLayer(_roiLayer, v);

    /// <summary>设置边缘点层内容（红色实心圆）</summary>
    public void SetEdgeVisual(DrawingVisual? v) => SetLayer(_edgeLayer, v);

    /// <summary>设置拟合直线层内容（绿色直线）</summary>
    public void SetFitLineVisual(DrawingVisual? v) => SetLayer(_lineLayer, v);

    /// <summary>清空所有层</summary>
    public void ClearAll()
    {
        _axisLayer.Children.Clear();
        _roiLayer.Children.Clear();
        _edgeLayer.Children.Clear();
        _lineLayer.Children.Clear();
    }

    /// <summary>仅清空检测结果层（边缘点 + 拟合直线）</summary>
    public void ClearResults()
    {
        _edgeLayer.Children.Clear();
        _lineLayer.Children.Clear();
    }

    private static void SetLayer(ContainerVisual layer, DrawingVisual? v)
    {
        layer.Children.Clear();
        if (v is not null) layer.Children.Add(v);
    }
}
