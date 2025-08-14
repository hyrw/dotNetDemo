using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfViewModelLocator.ViewModel;

public partial class ViewAViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = "Test ViewModel Locator!";
}
