using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DragDemo.Controls;

/// <summary>
/// DragCanvas.xaml 的交互逻辑
/// </summary>
public partial class DragCanvas : UserControl
{
    public bool IsChildHitTestVisible
    {
        get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
        set { SetValue(IsChildHitTestVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsChildHitTestVisibleProperty =
        DependencyProperty.Register(nameof(IsChildHitTestVisible), typeof(bool), typeof(DragCanvas), new PropertyMetadata(true));

    public DragCanvas()
    {
        InitializeComponent();
    }

    private void Rectangle_MouseMove(object sender, MouseEventArgs e)
    {
        if (Mouse.LeftButton != MouseButtonState.Pressed) return;
       
        IsChildHitTestVisible = false;
        DragDrop.DoDragDrop(rect, new DataObject(DataFormats.Serializable, rect), DragDropEffects.Move);
        IsChildHitTestVisible = true;
    }

    private void Canvas_Drop(object sender, DragEventArgs e)
    {
        var obj = e.Data.GetData(DataFormats.Serializable);
        if (obj is not UIElement uiElement) return;

        if (!canvas.Children.Contains(uiElement))
        {
            var point = e.GetPosition(canvas);
            Canvas.SetLeft(uiElement, point.X);
            Canvas.SetTop(uiElement, point.Y);
            canvas.Children.Add(uiElement);
        }
    }

    private void Canvas_DragLeave(object sender, DragEventArgs e)
    {
        if (e.OriginalSource != canvas) return;

        var obj = e.Data.GetData(DataFormats.Serializable);
        if (obj is not UIElement uiElement) return;

        canvas.Children.Remove(uiElement);
    }

    private void Canvas_DragOver(object sender, DragEventArgs e)
    {
        var obj = e.Data.GetData(DataFormats.Serializable);
        if (obj is not UIElement uiElement) return;

        var point = e.GetPosition(canvas);
        Canvas.SetLeft(uiElement, point.X);
        Canvas.SetTop(uiElement, point.Y);
        if (!canvas.Children.Contains(uiElement))
        {
            canvas.Children.Add(uiElement);
        }
    }
}
