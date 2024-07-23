using CommunityToolkit.Mvvm.ComponentModel;
using ControlDemoApp.Views;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ControlDemoApp.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{
    readonly IServiceProvider serviceProvider;
    private readonly DispatcherTimer timer;

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
        this.timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(2);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            if (Num == 9)
            {
                Num = 0;
            }
            else
            {
                Num++;
            }
        });
    }
}
