using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WpfViewModelLocator.ViewModel;

public partial class ViewAViewModel(ILogger<ViewAViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; } = "Test ViewModel Locator!";
}
