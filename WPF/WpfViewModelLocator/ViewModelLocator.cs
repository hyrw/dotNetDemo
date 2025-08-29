using CommunityToolkit.Mvvm.DependencyInjection;
using System.Windows.Controls;
using WpfViewModelLocator.ViewModel;

namespace WpfViewModelLocator;

public class ViewModelLocator
{
    public ViewAViewModel ViewAViewModel => Ioc.Default.GetRequiredService<ViewAViewModel>();

    public RichTextBox LogTextBox => Ioc.Default.GetRequiredService<RichTextBox>();
}
