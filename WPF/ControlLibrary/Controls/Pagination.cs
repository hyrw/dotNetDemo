using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlLibrary.Controls;

public class Pagination : Control
{
    public static readonly RoutedEvent PageIndexChangedEvent =
         EventManager.RegisterRoutedEvent(nameof(PageIndexChanged), RoutingStrategy.Bubble,
         typeof(PageIndexChangedEventHandler), typeof(Pagination));

    public delegate void PageIndexChangedEventHandler(object sender, PageIndexChangedEventArgs e);

    public event PageIndexChangedEventHandler PageIndexChanged
    {
        add { AddHandler(PageIndexChangedEvent, value); }
        remove { RemoveHandler(PageIndexChangedEvent, value); }
    }

    public ObservableCollection<int> Pages
    {
        get { return (ObservableCollection<int>)GetValue(PagesProperty); }
        set { SetValue(PagesProperty, value); }
    }
    public static readonly DependencyProperty PagesProperty =
        DependencyProperty.Register(nameof(Pages), typeof(ObservableCollection<int>), typeof(Pagination), new PropertyMetadata(new ObservableCollection<int>()));

    public int PageIndex
    {
        get { return (int)GetValue(PageIndexProperty); }
        set { SetValue(PageIndexProperty, value); }
    }
    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(Pagination), new PropertyMetadata(0, OnPageIndexChanged));

    private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Pagination p) return;
        int oldPageIndex = (int)e.OldValue;
        int newPageIndex = (int)e.NewValue;
        var args = new PageIndexChangedEventArgs(PageIndexChangedEvent, p)
        {
            OldValue = oldPageIndex,
            NewValue = newPageIndex,
        };
        p.RaiseEvent(args);
    }

    public int PageCount
    {
        get { return (int)GetValue(PageCountProperty); }
        set { SetValue(PageCountProperty, value); }
    }
    public static readonly DependencyProperty PageCountProperty =
        DependencyProperty.Register(nameof(PageCount), typeof(int), typeof(Pagination), new PropertyMetadata(0, OnPageCountChanged));

    private static void OnPageCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        Pagination p = (Pagination)d;
        p.Pages.Clear();
        for (int i = 1; i <= p.PageCount; i++)
        {
            p.Pages.Add(i);
        }
    }

    public Pagination()
    {
        CommandBindings.Add(new CommandBinding(NavigationCommands.FirstPage, ExecFirstPage, PageIndexGtOne));
        CommandBindings.Add(new CommandBinding(NavigationCommands.LastPage, ExecLastPage, PageIndexLtPageCount));
        CommandBindings.Add(new CommandBinding(NavigationCommands.PreviousPage, ExecPrevPage, PageIndexGtOne));
        CommandBindings.Add(new CommandBinding(NavigationCommands.NextPage, ExecNextPage, PageIndexLtPageCount));
        CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, ExecGotoPage, CanExecGotoPage));
    }

    private void CanExecGotoPage(object sender, CanExecuteRoutedEventArgs e)
    {
        try
        {
            int page = Convert.ToInt32(e.Parameter);
            if (1 <= page && page <= PageCount && PageIndex != page)
            {
                e.CanExecute = true;
            }
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void ExecGotoPage(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            int page = Convert.ToInt32(e.Parameter);
            if (1 <= page && page <= PageCount && PageIndex != page)
            {
                PageIndex = page;
            }
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void ExecNextPage(object sender, ExecutedRoutedEventArgs e)
    {
        if (PageIndex < PageCount)
        {
            PageIndex++;
        }
        e.Handled = true;
    }

    private void ExecPrevPage(object sender, ExecutedRoutedEventArgs e)
    {
        if (PageIndex > 1)
        {
            PageIndex--;
        }
        e.Handled = true;
    }

    private void ExecLastPage(object sender, ExecutedRoutedEventArgs e)
    {
        if (PageIndex != PageCount)
        {
            PageIndex = PageCount;
        }
        e.Handled = true;
    }

    private void PageIndexLtPageCount(object sender, CanExecuteRoutedEventArgs e)
    {
        if (PageIndex < PageCount)
        {
            e.CanExecute = true;
        }
        e.Handled = true;
    }


    private void PageIndexGtOne(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = PageIndex switch
        {
            > 1 => true,
            _ => false,
        };
        e.Handled = true;
    }

    private void ExecFirstPage(object sender, ExecutedRoutedEventArgs e)
    {
        if (PageIndex != 1)
        {
            PageIndex = 1;
        }
        e.Handled = true;
    }

    static Pagination()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Pagination), new FrameworkPropertyMetadata(typeof(Pagination)));
    }
}

public class PageIndexChangedEventArgs : RoutedEventArgs
{
    public int OldValue { get; set; }
    public int NewValue { get; set; }

    public PageIndexChangedEventArgs() : base() { }
    public PageIndexChangedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    public PageIndexChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
}
