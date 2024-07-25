using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty]
    int num = 0;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private void ChangeNumber()
    {
        Num = Random.Shared.Next(0, 10);
    }
}
