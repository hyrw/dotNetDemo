using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ControlLibrary.Controls;

[TemplatePart(Name = PART_PagesContainer, Type = typeof(ItemsControl))]
public class Pagination : Control
{
    #region const
    const string PART_PagesContainer = "PART_PagesContainer";
    #endregion

    readonly ObservableCollection<PaginationItem> pages = [];

    bool isApplyTemplate;

    #region routed event
    public static readonly RoutedEvent PageIndexChangedEvent =
         EventManager.RegisterRoutedEvent(nameof(PageIndexChanged), RoutingStrategy.Bubble,
         typeof(PageIndexChangedEventHandler), typeof(Pagination));

    public delegate void PageIndexChangedEventHandler(object sender, PageIndexChangedEventArgs e);

    public event PageIndexChangedEventHandler PageIndexChanged
    {
        add { AddHandler(PageIndexChangedEvent, value); }
        remove { RemoveHandler(PageIndexChangedEvent, value); }
    }
    #endregion

    #region dependency property
    public int PageIndex
    {
        get { return (int)GetValue(PageIndexProperty); }
        set { SetValue(PageIndexProperty, value); }
    }
    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(Pagination), new PropertyMetadata(0, OnPageIndexChanged));
    #endregion

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

        p.UpdatePageList();
    }

    void UpdatePageList()
    {
        if (!this.isApplyTemplate) return;

        this.pages.Clear();
        for (int i = 1; i <= this.PageCount; i++)
        {
            this.pages.Add(new PaginationItem
            {
                PageIndex = i,
                Text = i.ToString(),
            });
        }
    }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public Pagination()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    {
        CommandBindings.Add(new CommandBinding(NavigationCommands.FirstPage, ExecFirstPage, PageIndexGtOne));
        CommandBindings.Add(new CommandBinding(NavigationCommands.LastPage, ExecLastPage, PageIndexLtPageCount));
        CommandBindings.Add(new CommandBinding(NavigationCommands.PreviousPage, ExecPrevPage, PageIndexGtOne));
        CommandBindings.Add(new CommandBinding(NavigationCommands.NextPage, ExecNextPage, PageIndexLtPageCount));
        CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, ExecGotoPage, CanExecGotoPage));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var pagesContainer = (ItemsControl)GetTemplateChild(PART_PagesContainer);

        pagesContainer.SetBinding(ItemsControl.ItemsSourceProperty, new Binding()
        {
            Source = this.pages,
        });

        this.isApplyTemplate = true;
        UpdatePageList();
    }

    private void CanExecGotoPage(object sender, CanExecuteRoutedEventArgs e)
    {
        try
        {
            if (e.Parameter is not PaginationItem pageItem) return;

            int page = pageItem.PageIndex;

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
        e.Handled = true;
        if (e.Parameter is not PaginationItem pageItem) return;

        int page = pageItem.PageIndex;
        if (1 <= page && page <= PageCount && PageIndex != page)
        {
            PageIndex = page;
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

public class PaginationItem
{
    public int PageIndex { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class PageIndexChangedEventArgs : RoutedEventArgs
{
    public int OldValue { get; set; }
    public int NewValue { get; set; }

    public PageIndexChangedEventArgs() : base() { }
    public PageIndexChangedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
    public PageIndexChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
}
