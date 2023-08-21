using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFUserControlDemo.UserControls;

// https://stackoverflow.com/questions/29126224/how-do-i-bind-wpf-commands-between-a-usercontrol-and-a-parent-window
/// <summary>
/// ItemsEdittor.xaml 的交互逻辑
/// </summary>
public partial class ItemsEditor : UserControl
{
    #region Items
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(IEnumerable<string>),
            typeof(ItemsEditor),
            new UIPropertyMetadata(null));
    public IEnumerable<string> Items
    {
        get { return (IEnumerable<string>)GetValue(ItemsProperty); }
        set { SetValue(ItemsProperty, value); }
    }
    #endregion
    #region AddItem
    public static readonly DependencyProperty AddItemProperty =
        DependencyProperty.Register(
            nameof(AddItem),
            typeof(ICommand),
            typeof(ItemsEditor),
            new UIPropertyMetadata(null));
    public ICommand AddItem
    {
        get { return (ICommand)GetValue(AddItemProperty); }
        set { SetValue(AddItemProperty, value); }
    }
    #endregion
    #region RemoveItem
    public static readonly DependencyProperty RemoveItemProperty =
        DependencyProperty.Register(
            "RemoveItem",
            typeof(ICommand),
            typeof(ItemsEditor),
            new UIPropertyMetadata(null));
    public ICommand RemoveItem
    {
        get { return (ICommand)GetValue(RemoveItemProperty); }
        set { SetValue(RemoveItemProperty, value); }
    }
    #endregion
    public ItemsEditor()
    {
        InitializeComponent();
    }
}
