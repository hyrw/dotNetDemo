using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DragDemo.Controls;

/// <summary>
/// DropZoon.xaml 的交互逻辑
/// </summary>
public partial class DropZoon : UserControl
{
    readonly DoubleCollection strokeDashArray;
    public DropZoon()
    {
        InitializeComponent();
        strokeDashArray = [4, 2];
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);
        if (e.Data.GetDataPresent(typeof(Brush)))
        {
            var brush = (Brush)e.Data.GetData(typeof(Brush));
            this.borderRect.Fill = brush;
            this.borderRect.Stroke = Brushes.Black;
            this.borderRect.StrokeDashArray = null;
        }
        e.Handled = true;
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);
        this.borderRect.Stroke = Brushes.Black;
        this.borderRect.StrokeDashArray = null;
        e.Handled = true;
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        base.OnDragEnter(e);
        if (e.Data.GetDataPresent(typeof(Brush)))
        {
            var brush = (Brush)e.Data.GetData(typeof(Brush));
            this.borderRect.Stroke = brush;
            this.borderRect.StrokeDashArray = strokeDashArray;
        }
        e.Handled = true;
    }
}
