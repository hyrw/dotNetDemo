using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ControlLibrary.Controls;

public class AutoCompleteBox : Control
{
    TextBox inputText;
    ListBox suggestionBox;
    ICollectionView suggestionView;

    static AutoCompleteBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteBox), new FrameworkPropertyMetadata(typeof(AutoCompleteBox)));
    }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        inputText = (TextBox)GetTemplateChild("inputText");
        suggestionBox = (ListBox)GetTemplateChild("suggestionBox");
        //suggestionView = CollectionViewSource.GetDefaultView(ItemsSource);
        //suggestionView.Filter = filterSuggestion;
        inputText.TextChanged += updateSuggestion;
        inputText.KeyUp += onKeyUp;
        //SelectItem = new Command(selectItem, (o) => IsSuggestionVisible);
    }

    void onKeyUp(object sender, KeyEventArgs e)
    {
        if (IsSuggestionVisible)
        {
            suggestionView.MoveCurrentToFirst();
            ((ListBoxItem)suggestionBox.ItemContainerGenerator.ContainerFromItem(suggestionView.CurrentItem)).Focus();
            var a =new CollectionViewSource();
        }
    }

    bool filterSuggestion(object o) => (((string)o).ToLower()).Contains(inputText.Text.ToLower());
    void updateSuggestion(object sender, TextChangedEventArgs e)
    {
        if (suggestionView is null) return;
        suggestionView.Refresh();
        IsSuggestionVisible = string.IsNullOrWhiteSpace(inputText.Text) ? false : true;
    }
    void selectItem(object o)
    {
        var text = (string)o;
        inputText.Text = text;
        inputText.CaretIndex = text.Length + 1;
        inputText.Focus();
        IsSuggestionVisible = false;
    }

    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...  
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteBox), new PropertyMetadata(null));


    public IEnumerable ItemsSource
    {
        get { return (IEnumerable)GetValue(ItemsSourceProperty); }
        set { SetValue(ItemsSourceProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...  
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoCompleteBox), new PropertyMetadata(null, OnItemsSourceChangedCallback));

    private static void OnItemsSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AutoCompleteBox autoCompleteBox) return;
        autoCompleteBox.SetFilterView();
    }

    private void SetFilterView()
    {
        if (ItemsSource == null) return;
        if (suggestionView != null)
        {
            suggestionView.Filter -= filterSuggestion;
        }

        suggestionView = CollectionViewSource.GetDefaultView(ItemsSource);
        suggestionView.Filter += filterSuggestion;
    }

    public bool IsSuggestionVisible
    {
        get { return (bool)GetValue(IsSuggestionVisibleProperty); }
        set { SetValue(IsSuggestionVisibleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsSuggestionVisible.  This enables animation, styling, binding, etc...  
    public static readonly DependencyProperty IsSuggestionVisibleProperty =
        DependencyProperty.Register("IsSuggestionVisible", typeof(bool), typeof(AutoCompleteBox), new PropertyMetadata(false));


    public ICommand SelectItem
    {
        get { return (ICommand)GetValue(SelectItemProperty); }
        set { SetValue(SelectItemProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SelectItem.  This enables animation, styling, binding, etc...  
    public static readonly DependencyProperty SelectItemProperty =
        DependencyProperty.Register("SelectItem", typeof(ICommand), typeof(AutoCompleteBox), new PropertyMetadata(null));
}
