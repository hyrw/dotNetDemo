using CommunityToolkit.Mvvm.DependencyInjection;
using WpfViewModelLocator.ViewModel;

namespace WpfViewModelLocator;

public class ViewModelLocator
{
    public ViewAViewModel ViewAViewModel => Ioc.Default.GetRequiredService<ViewAViewModel>();
}
