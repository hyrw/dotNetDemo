using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Channels;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NavigationDemo.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private int count;

    private readonly ChannelReader<int> channelReader;
    private readonly System.Timers.Timer timer;

    public ICommand CountAddCommand { get; set; }

    public MainWindowViewModel(ChannelReader<int> channelWriter)
    {
        CountAddCommand = new RelayCommand(() => Count++);
        this.channelReader = channelWriter;
        timer = new System.Timers.Timer()
        {
            Interval = 10,
            AutoReset = true,
        };
        timer.Elapsed += Timer_ElapsedAsync;
        timer.Start();
    }

    private void Timer_ElapsedAsync(object? sender, ElapsedEventArgs e)
    {
        if (!channelReader.TryRead(out int n)) return;
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            Count = n;
        });
    }
}
