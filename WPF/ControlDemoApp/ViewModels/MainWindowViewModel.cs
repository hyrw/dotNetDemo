using CommunityToolkit.Mvvm.ComponentModel;
using ControlDemoApp.Views;
using System.Windows.Controls;

namespace ControlDemoApp.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    readonly IServiceProvider serviceProvider;

    public Page HPBarView
    {
        get => serviceProvider.GetRequiredService<HPBarView>();
        //set => SetProperty(ref hpBarView, value);
    }
    //Page hpBarView;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
}
