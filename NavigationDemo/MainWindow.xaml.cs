using Microsoft.Extensions.Hosting;
using NavigationDemo.ViewModel;
using System.Windows;

namespace NavigationDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IHostApplicationLifetime hostApplicationLifetime;

    public MainWindow(IHostApplicationLifetime hostApplicationLifetime, MainWindowViewModel viewModel)
    {
        InitializeComponent();
        this.hostApplicationLifetime = hostApplicationLifetime;
        DataContext = viewModel;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        hostApplicationLifetime.StopApplication();
    }
}