using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DragDemo.Controls;

/// <summary>
/// Draggable.xaml 的交互逻辑
/// </summary>
public partial class Draggable : UserControl
{
    DraggableAdorner _draggableAdorner;
    public Draggable()
    {
        InitializeComponent();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (e.LeftButton != MouseButtonState.Pressed) return;

        var data = new DataObject();
        data.SetData(typeof(Brush), this.Background);

        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        _draggableAdorner = new DraggableAdorner(this);
        adornerLayer.Add(_draggableAdorner);

        DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);

        adornerLayer.Remove(_draggableAdorner);

        e.Handled = true;
    }

    protected override void OnPreviewGiveFeedback(GiveFeedbackEventArgs e)
    {
        // todo: 拖动预览
        base.OnPreviewGiveFeedback(e);
        Point point = Mouse.GetPosition(this);
        _draggableAdorner.Arrange(new Rect(point, _draggableAdorner.RenderSize));
        Trace.WriteLine($"{DateTime.Now} GiveFeedback point {point}");
    }

    private class DraggableAdorner : Adorner
    {
        readonly Draggable _adornedElement;

        Rect _rect;
        Brush _brush;

        public DraggableAdorner(Draggable adornedElement) : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _rect = new Rect(_adornedElement.RenderSize);
            _brush = _adornedElement.Background.Clone();
            this.IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(_brush, null, _rect);
        }
    }
}
