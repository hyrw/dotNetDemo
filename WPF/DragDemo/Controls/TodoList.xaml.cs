using DragDemo.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DragDemo.Controls;

/// <summary>
/// TodoList.xaml 的交互逻辑
/// </summary>
public partial class TodoList : UserControl
{

    public IEnumerable<TodoItemModel> ItemsSource
    {
        get { return (IEnumerable<TodoItemModel>)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<TodoItemModel>), typeof(TodoList), new PropertyMetadata());

    public ICommand? AddCommand
    {
        get { return (ICommand)GetValue(AddCommandProperty); }
        set { SetValue(AddCommandProperty, value); }
    }
    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(TodoList), new PropertyMetadata(null));

    public ICommand? RemoveCommand
    {
        get { return (ICommand)GetValue(RemoveCommandProperty); }
        set { SetValue(AddCommandProperty, value); }
    }
    public static readonly DependencyProperty RemoveCommandProperty =
        DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(TodoList), new PropertyMetadata(null));


    public TodoList()
    {
        InitializeComponent();
    }

    private void ListViewItem_MouseMove(object sender, MouseEventArgs e)
    {
        e.Handled = true;
        if (Mouse.LeftButton == MouseButtonState.Pressed &&
            sender is FrameworkElement frameworkElement)
        {
            object todoItem = frameworkElement.DataContext;
            DataObject data = new(typeof(TodoItemModel), todoItem);
            DragDropEffects result = DragDrop.DoDragDrop(frameworkElement, data, DragDropEffects.Move);
            if (result == DragDropEffects.None)
            {
                AddCommand?.Execute(todoItem);
            }
        }
    }

    private void ListView_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        Type format = typeof(TodoItemModel);
        if (e.Data.GetDataPresent(format))
        {
            TodoItemModel todoItem = (TodoItemModel)e.Data.GetData(format);
            if (AddCommand?.CanExecute(null) ?? false)
            {
                AddCommand.Execute(todoItem);
            }
        }
    }

    private void ListView_DragLeave(object sender, DragEventArgs e)
    {
        e.Handled = true;
        HitTestResult result = VisualTreeHelper.HitTest(list, e.GetPosition(list));
        if (result != null) return;
        Type format = typeof(TodoItemModel);
        if (e.Data.GetDataPresent(format))
        {
            var todoItem = (TodoItemModel)e.Data.GetData(format);

            if (RemoveCommand?.CanExecute(null) ?? false)
            {
                RemoveCommand.Execute(todoItem);
            }
        }
    }

    private void ListViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if (sender is FrameworkElement frameworkElement) {
            var insert = frameworkElement.DataContext;
            var data = e.Data.GetData(DataFormats.Serializable);
        }
    }
}
